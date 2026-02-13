using Atlas.Common.Validation.Core;

namespace Atlas.Common.Validation.Rules;

/// <summary>
/// Represents a validation rule that can be applied to an instance of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IValidationRule<in T>
{
    /// <summary>
    /// Gets the property name this rule validates, or <c>null</c> if the rule applies to the whole object.
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    /// Validates the specified instance and returns any failures.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A collection of validation failures. Empty if valid.</returns>
    IEnumerable<ValidationFailure> Validate(T instance);

    /// <summary>
    /// Validates the specified instance within a validation context and returns any failures.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A collection of validation failures. Empty if valid.</returns>
    IEnumerable<ValidationFailure> Validate(T instance, IValidationContext context);
}
