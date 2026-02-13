using Atlas.Common.Validation.Core;
using Microsoft.AspNetCore.Components.Forms;

namespace Atlas.Common.Validation.Blazor;

/// <summary>
/// Extension methods for integrating Atlas.Common.Validation with Blazor's <see cref="EditContext"/>.
/// </summary>
public static class EditContextExtensions
{
    /// <summary>
    /// Adds Atlas.Common.Validation support to an <see cref="EditContext"/>.
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
        ArgumentNullException.ThrowIfNull(argument: editContext);
        ArgumentNullException.ThrowIfNull(argument: validator);

        var messageStore = new ValidationMessageStore(editContext: editContext);

        editContext.OnValidationRequested += (sender, _) =>
        {
            messageStore.Clear();
            if (sender is EditContext ctx && ctx.Model is T model)
            {
                var result = validator.Validate(instance: model);
                PopulateMessageStore(messageStore: messageStore, editContext: ctx, result: result);
            }
        };

        editContext.OnFieldChanged += (sender, args) =>
        {
            messageStore.Clear(fieldIdentifier: args.FieldIdentifier);
            if (sender is EditContext ctx && ctx.Model is T model)
            {
                var result = validator.Validate(instance: model);
                var fieldErrors = result.Errors
                    .Where(predicate: e => string.Equals(a: e.PropertyName, b: args.FieldIdentifier.FieldName, comparisonType: StringComparison.OrdinalIgnoreCase));

                foreach (var error in fieldErrors)
                {
                    messageStore.Add(fieldIdentifier: args.FieldIdentifier, message: FormatMessage(error: error));
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
            var fieldIdentifier = new FieldIdentifier(model: editContext.Model, fieldName: error.PropertyName);
            messageStore.Add(fieldIdentifier: fieldIdentifier, message: FormatMessage(error: error));
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
