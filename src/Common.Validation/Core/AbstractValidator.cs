using System.Linq.Expressions;
using System.Reflection;
using Common.Validation.Layers;
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
    private static readonly string? CachedLayer = typeof(T)
        .GetCustomAttribute<ValidationLayerAttribute>(inherit: true)?.Layer;

    /// <summary>
    /// Gets or sets the cascade mode for this validator.
    /// When set to <see cref="CascadeMode.StopOnFirstFailure"/>,
    /// the validator stops after the first rule that produces a failure.
    /// Default is <see cref="CascadeMode.Continue"/>.
    /// </summary>
    public CascadeMode CascadeMode { get; set; } = CascadeMode.Continue;

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

    #region IValidator<T>

    /// <inheritdoc />
    public ValidationResult Validate(T instance)
    {
        return Validate(instance, new ValidationContext(CachedLayer));
    }

    /// <inheritdoc />
    public ValidationResult Validate(T instance, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(context);

        var failures = new List<ValidationFailure>();

        foreach (var rule in _rules)
        {
            var ruleFailures = rule.Validate(instance, context);
            failures.AddRange(ruleFailures);

            if (CascadeMode == CascadeMode.StopOnFirstFailure && failures.Count > 0)
                break;
        }

        return new ValidationResult(failures);
    }

    #endregion

    #region IValidator (non-generic)

    /// <inheritdoc />
    Type IValidator.ValidatedType => typeof(T);

    /// <inheritdoc />
    ValidationResult IValidator.Validate(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (instance is not T typed)
            throw new ArgumentException(
                $"Expected instance of type '{typeof(T).FullName}' but received '{instance.GetType().FullName}'.",
                nameof(instance));

        return Validate(typed);
    }

    /// <inheritdoc />
    ValidationResult IValidator.Validate(object instance, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (instance is not T typed)
            throw new ArgumentException(
                $"Expected instance of type '{typeof(T).FullName}' but received '{instance.GetType().FullName}'.",
                nameof(instance));

        return Validate(typed, context);
    }

    #endregion
}
