using Common.Validation.Core;

namespace Common.Validation.Rules;

/// <summary>
/// Represents a standalone validation rule that validates a value directly,
/// without requiring a parent object or container.
/// </summary>
/// <typeparam name="TProperty">The type of the value being validated.</typeparam>
public interface IValueValidationRule<in TProperty>
{
    /// <summary>
    /// Gets the property name associated with this rule for error reporting.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Validates the specified value and returns any failures.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>A collection of validation failures. Empty if valid.</returns>
    IEnumerable<ValidationFailure> Validate(TProperty value);

    /// <summary>
    /// Validates the specified value within a validation context and returns any failures.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">The validation context carrying layer and custom data.</param>
    /// <returns>A collection of validation failures. Empty if valid.</returns>
    IEnumerable<ValidationFailure> Validate(TProperty value, IValidationContext context);
}
