using Common.Validation.Rules;

namespace Common.Validation.Core;

/// <summary>
/// Internal value validator that configures rules via a delegate.
/// Used by <see cref="ValueValidator.Create{TProperty}"/>.
/// </summary>
internal sealed class InlineValueValidator<TProperty> : ValueValidator<TProperty>
{
    public InlineValueValidator(Action<IValueRuleBuilder<TProperty>> configure, string? propertyName = null)
        : base(defaultPropertyName: propertyName)
    {
        configure(obj: Check());
    }
}
