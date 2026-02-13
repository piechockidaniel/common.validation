namespace Atlas.Common.Validation.Core;

/// <summary>
/// Non-generic base interface for standalone value validators.
/// Enables DI container registration and runtime resolution without knowing
/// the validated type at compile time.
/// </summary>
public interface IValueValidator
{
    /// <summary>
    /// Gets the type this validator can validate.
    /// </summary>
    Type ValidatedType { get; }

    /// <summary>
    /// Validates the specified value and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="value">The value to validate (may be <c>null</c> for reference types).</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(object? value);

    /// <summary>
    /// Validates the specified value within a validation context.
    /// </summary>
    /// <param name="value">The value to validate (may be <c>null</c> for reference types).</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(object? value, IValidationContext context);
}

/// <summary>
/// Defines a standalone validator for values of type <typeparamref name="TProperty"/>.
/// Unlike <see cref="IValidator{T}"/>, the value is validated independently —
/// no parent object or container is required.
/// </summary>
/// <typeparam name="TProperty">The type of the value being validated.</typeparam>
public interface IValueValidator<in TProperty> : IValueValidator
{
    /// <summary>
    /// Validates the specified value and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(TProperty value);

    /// <summary>
    /// Validates the specified value within a validation context.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(TProperty value, IValidationContext context);
}
