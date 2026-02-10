namespace Common.Validation.Json.Registry;

/// <summary>
/// Represents a single validation check that operates on an object property value.
/// Used by JSON-based validators to execute rules resolved from the validator type registry.
/// </summary>
public interface IPropertyCheck
{
    /// <summary>
    /// Validates the given property value.
    /// </summary>
    /// <param name="value">The property value to validate (may be <c>null</c>).</param>
    /// <returns><c>true</c> if the value is valid; otherwise, <c>false</c>.</returns>
    bool IsValid(object? value);
}
