using System.Linq.Expressions;

namespace Common.Validation.Core;

/// <summary>
/// Internal interface for validators that support property-level validation.
/// Implemented by <see cref="AbstractValidator{T}"/> and <see cref="Json.JsonValidator{T}"/>.
/// </summary>
internal interface IPropertyValidator<T> : IValidator<T>
{
    ValidationResult ValidateProperty<TProperty>(T instance, Expression<Func<T, TProperty>> propertyExpression, IValidationContext context);
}
