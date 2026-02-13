namespace Atlas.Common.Validation.Core;

/// <summary>
/// Non-generic base interface for validators.
/// Enables DI container registration and runtime resolution without knowing the validated type at compile time.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Gets the type this validator can validate.
    /// </summary>
    Type ValidatedType { get; }

    /// <summary>
    /// Validates the specified instance and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(object instance);

    /// <summary>
    /// Validates the specified instance within a validation context.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(object instance, IValidationContext context);
}

/// <summary>
/// Defines a validator for instances of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object being validated.</typeparam>
public interface IValidator<in T> : IValidator
{
    /// <summary>
    /// Validates the specified instance and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(T instance);

    /// <summary>
    /// Validates the specified instance within a validation context.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(T instance, IValidationContext context);
}
