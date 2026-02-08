using Common.Validation.Core;

namespace Common.Validation.Rules;

/// <summary>
/// Represents a validation rule that can be applied to an instance of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IValidationRule<in T>
{
    /// <summary>
    /// Validates the specified instance and returns any failures.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A collection of validation failures. Empty if valid.</returns>
    IEnumerable<ValidationFailure> Validate(T instance);
}
