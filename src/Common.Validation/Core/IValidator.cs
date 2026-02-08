namespace Common.Validation.Core;

/// <summary>
/// Defines a validator for instances of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object being validated.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the specified instance and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation failures.</returns>
    ValidationResult Validate(T instance);
}
