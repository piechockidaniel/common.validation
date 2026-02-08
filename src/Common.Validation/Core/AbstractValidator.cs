using System.Linq.Expressions;
using Common.Validation.Rules;

namespace Common.Validation.Core;

/// <summary>
/// Base class for building validators using a fluent builder pattern.
/// Inherit from this class and define rules in the constructor.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly List<IValidationRule<T>> _rules = new();

    /// <summary>
    /// Defines a validation rule for a specific property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="expression">A lambda expression pointing to the property to validate.</param>
    /// <returns>An <see cref="IRuleBuilder{T, TProperty}"/> for fluent rule chaining.</returns>
    protected IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var rule = new PropertyRule<T, TProperty>(expression);
        _rules.Add(rule);
        return rule;
    }

    /// <inheritdoc />
    public ValidationResult Validate(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var failures = new List<ValidationFailure>();

        foreach (var rule in _rules)
        {
            failures.AddRange(rule.Validate(instance));
        }

        return new ValidationResult(failures);
    }
}
