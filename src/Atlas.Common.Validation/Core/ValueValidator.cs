using Atlas.Common.Validation.Rules;

namespace Atlas.Common.Validation.Core;

/// <summary>
/// Base class for building standalone value validators using a fluent builder pattern.
/// Inherit from this class and define rules in the constructor, or use the static
/// <see cref="ValueValidator.Create{TProperty}"/> factory method for inline definitions.
/// <para>
/// Unlike <see cref="AbstractValidator{T}"/>, this validator does not require a parent object —
/// the value is validated independently.
/// </para>
/// </summary>
/// <typeparam name="TProperty">The type of the value being validated.</typeparam>
/// <example>
/// <code>
/// // Class-based definition:
/// public class EmailValidator : ValueValidator&lt;string&gt;
/// {
///     public EmailValidator() : base("Email")
///     {
///         Check().NotEmpty().EmailAddress().MaxLength(255);
///     }
/// }
///
/// // Usage:
/// var validator = new EmailValidator();
/// var result = validator.Validate("user@example.com");
/// </code>
/// </example>
public abstract class ValueValidator<TProperty> : IValueValidator<TProperty>
{
    private readonly List<IValueValidationRule<TProperty>> _rules = [];

    /// <summary>
    /// Gets or sets the cascade mode for this validator.
    /// When set to <see cref="CascadeMode.StopOnFirstFailure"/>,
    /// the validator stops after the first rule that produces a failure.
    /// Default is <see cref="CascadeMode.Continue"/>.
    /// </summary>
    public CascadeMode CascadeMode { get; set; } = CascadeMode.Continue;

    /// <summary>
    /// Gets the default property name used in validation failure reports.
    /// </summary>
    protected string DefaultPropertyName { get; }

    /// <summary>
    /// Creates a new <see cref="ValueValidator{TProperty}"/> with an optional default property name.
    /// </summary>
    /// <param name="defaultPropertyName">
    /// The property name to use in validation failures.
    /// When <c>null</c>, defaults to the type name of <typeparamref name="TProperty"/>.
    /// </param>
    protected ValueValidator(string? defaultPropertyName = null)
    {
        DefaultPropertyName = defaultPropertyName ?? typeof(TProperty).Name;
    }

    /// <summary>
    /// Defines a validation rule chain for the value.
    /// </summary>
    /// <param name="propertyName">
    /// Optional property name override for this rule.
    /// When <c>null</c>, uses <see cref="DefaultPropertyName"/>.
    /// </param>
    /// <returns>An <see cref="IValueRuleBuilder{TProperty}"/> for fluent rule chaining.</returns>
    protected IValueRuleBuilder<TProperty> Check(string? propertyName = null)
    {
        var rule = new ValueRule<TProperty>(propertyName: propertyName ?? DefaultPropertyName);
        _rules.Add(item: rule);
        return rule;
    }

    #region IValueValidator<TProperty>

    /// <inheritdoc />
    public ValidationResult Validate(TProperty value)
    {
        return Validate(value: value, context: new ValidationContext());
    }

    /// <inheritdoc />
    public ValidationResult Validate(TProperty value, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);

        var failures = new List<ValidationFailure>();

        foreach (var rule in _rules)
        {
            var ruleFailures = rule.Validate(value: value, context: context);
            failures.AddRange(collection: ruleFailures);

            if (CascadeMode == CascadeMode.StopOnFirstFailure && failures.Count > 0)
                break;
        }

        return new ValidationResult(errors: failures);
    }

    #endregion

    #region IValueValidator (non-generic)

    /// <inheritdoc />
    Type IValueValidator.ValidatedType => typeof(TProperty);

    /// <inheritdoc />
    ValidationResult IValueValidator.Validate(object? value)
    {
        return ((IValueValidator)this).Validate(value: value, context: new ValidationContext());
    }

    /// <inheritdoc />
    ValidationResult IValueValidator.Validate(object? value, IValidationContext context)
    {
        if (value is TProperty typed)
            return Validate(value: typed, context: context);

        if (value is null && default(TProperty) is null)
            return Validate(value: default!, context: context);

        throw new ArgumentException(
            message: $"Expected value of type '{typeof(TProperty).FullName}' but received '{value?.GetType().FullName ?? "null"}'.",
            paramName: nameof(value));
    }

    #endregion
}

/// <summary>
/// Static factory for creating inline standalone value validators
/// without defining a dedicated class.
/// </summary>
/// <example>
/// <code>
/// var validator = ValueValidator.Create&lt;string&gt;(
///     configure: b => b.NotEmpty().EmailAddress(),
///     propertyName: "Email");
///
/// var result = validator.Validate("user@example.com");
/// </code>
/// </example>
public static class ValueValidator
{
    /// <summary>
    /// Creates a standalone value validator from a configuration action.
    /// </summary>
    /// <typeparam name="TProperty">The type of the value to validate.</typeparam>
    /// <param name="configure">An action that defines the validation rules on the builder.</param>
    /// <param name="propertyName">
    /// Optional property name for validation failure reports.
    /// When <c>null</c>, defaults to the type name of <typeparamref name="TProperty"/>.
    /// </param>
    /// <returns>A configured <see cref="IValueValidator{TProperty}"/>.</returns>
    public static IValueValidator<TProperty> Create<TProperty>(
        Action<IValueRuleBuilder<TProperty>> configure,
        string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(argument: configure);
        return new InlineValueValidator<TProperty>(configure: configure, propertyName: propertyName);
    }
}

