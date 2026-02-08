namespace Common.Validation.Core;

/// <summary>
/// Indicates the severity level of a validation failure.
/// </summary>
public enum Severity
{
    /// <summary>
    /// The value is technically valid but not recommended.
    /// Informational — the operation proceeds normally.
    /// </summary>
    NotRecommended = 0,

    /// <summary>
    /// The value is risky. The operation may proceed but the caller
    /// accepts responsibility for any consequences.
    /// </summary>
    AtOwnRisk = 1,

    /// <summary>
    /// The value is invalid. The operation must not proceed.
    /// </summary>
    Forbidden = 2
}
