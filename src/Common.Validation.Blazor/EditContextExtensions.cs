using Common.Validation.Core;
using Microsoft.AspNetCore.Components.Forms;

namespace Common.Validation.Blazor;

/// <summary>
/// Extension methods for integrating Common.Validation with Blazor's <see cref="EditContext"/>.
/// </summary>
public static class EditContextExtensions
{
    /// <summary>
    /// Adds Common.Validation support to an <see cref="EditContext"/>.
    /// Hooks into <see cref="EditContext.OnValidationRequested"/> and <see cref="EditContext.OnFieldChanged"/>
    /// to trigger validation and feed results back into the <see cref="ValidationMessageStore"/>.
    /// </summary>
    /// <typeparam name="T">The type of the model being edited.</typeparam>
    /// <param name="editContext">The Blazor edit context.</param>
    /// <param name="validator">The validator to use.</param>
    /// <returns>The edit context for chaining.</returns>
    public static EditContext AddCommonValidation<T>(
        this EditContext editContext,
        IValidator<T> validator) where T : class
    {
        ArgumentNullException.ThrowIfNull(editContext);
        ArgumentNullException.ThrowIfNull(validator);

        var messageStore = new ValidationMessageStore(editContext);

        editContext.OnValidationRequested += (sender, _) =>
        {
            messageStore.Clear();
            if (sender is EditContext ctx && ctx.Model is T model)
            {
                var result = validator.Validate(model);
                PopulateMessageStore(messageStore, ctx, result);
            }
        };

        editContext.OnFieldChanged += (sender, args) =>
        {
            messageStore.Clear(args.FieldIdentifier);
            if (sender is EditContext ctx && ctx.Model is T model)
            {
                var result = validator.Validate(model);
                var fieldErrors = result.Errors
                    .Where(e => string.Equals(e.PropertyName, args.FieldIdentifier.FieldName, StringComparison.OrdinalIgnoreCase));

                foreach (var error in fieldErrors)
                {
                    messageStore.Add(args.FieldIdentifier, FormatMessage(error));
                }
                ctx.NotifyValidationStateChanged();
            }
        };

        return editContext;
    }

    private static void PopulateMessageStore(
        ValidationMessageStore messageStore,
        EditContext editContext,
        ValidationResult result)
    {
        foreach (var error in result.Errors)
        {
            var fieldIdentifier = new FieldIdentifier(editContext.Model, error.PropertyName);
            messageStore.Add(fieldIdentifier, FormatMessage(error));
        }

        editContext.NotifyValidationStateChanged();
    }

    private static string FormatMessage(ValidationFailure error)
    {
        return error.Severity switch
        {
            Severity.Forbidden => $"[Forbidden] {error.ErrorMessage}",
            Severity.AtOwnRisk => $"[At Own Risk] {error.ErrorMessage}",
            Severity.NotRecommended => $"[Not Recommended] {error.ErrorMessage}",
            _ => error.ErrorMessage
        };
    }
}
