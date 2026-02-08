namespace Common.Validation.Core;

/// <summary>
/// Represents a single validation failure.
/// </summary>
public class ValidationFailure
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets or sets the error message describing the failure.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets an optional error code for programmatic handling.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the severity level of this failure.
    /// Defaults to <see cref="Core.Severity.Forbidden"/>.
    /// </summary>
    public Severity Severity { get; set; } = Severity.Forbidden;

    /// <summary>
    /// Gets the value that was validated.
    /// </summary>
    public object? AttemptedValue { get; }

    /// <summary>
    /// Creates a new <see cref="ValidationFailure"/>.
    /// </summary>
    /// <param name="propertyName">The property name that failed validation.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="attemptedValue">The value that was validated.</param>
    public ValidationFailure(string propertyName, string errorMessage, object? attemptedValue = null)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }

    /// <inheritdoc />
    public override string ToString() => $"[{Severity}] {PropertyName}: {ErrorMessage}";
}
