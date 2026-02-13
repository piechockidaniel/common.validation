using System.Linq.Expressions;
using Atlas.Common.Validation.Core;

namespace Atlas.Common.Validation.Extensions;

/// <summary>
/// Extension methods for property-level validation.
/// </summary>
public static class PropertyValidationExtensions
{
    /// <summary>
    /// Validates only the specified property of the instance.
    /// Does not run rules for other properties.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="validator">The validator.</param>
    /// <param name="instance">The object to validate.</param>
    /// <param name="propertyExpression">Expression identifying the property, e.g. <c>x => x.Email</c>.</param>
    /// <returns>A <see cref="ValidationResult"/> containing failures for the specified property only.</returns>
    public static ValidationResult ValidateProperty<T, TProperty>(
        this IValidator<T> validator,
        T instance,
        Expression<Func<T, TProperty>> propertyExpression)
    {
        return validator.ValidateProperty(instance, propertyExpression, context: new ValidationContext(layer: null));
    }

    /// <summary>
    /// Validates only the specified property of the instance within a validation context.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="validator">The validator.</param>
    /// <param name="instance">The object to validate.</param>
    /// <param name="propertyExpression">Expression identifying the property.</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A <see cref="ValidationResult"/> containing failures for the specified property only.</returns>
    public static ValidationResult ValidateProperty<T, TProperty>(
        this IValidator<T> validator,
        T instance,
        Expression<Func<T, TProperty>> propertyExpression,
        IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentNullException.ThrowIfNull(context);

        if (validator is IPropertyValidator<T> propertyValidator)
            return propertyValidator.ValidateProperty(instance, propertyExpression, context);

        var propertyName = PropertyExpressionHelper.GetPropertyName(propertyExpression);
        var fullResult = validator.Validate(instance, context);
        var filtered = fullResult.Errors
            .Where(e => string.Equals(e.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return new ValidationResult(errors: filtered);
    }
}
