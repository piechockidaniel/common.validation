namespace Common.Validation.Core;

/// <summary>
/// Determines how the validator behaves when a validation check fails.
/// </summary>
public enum CascadeMode
{
    /// <summary>
    /// Continue executing all checks even if some fail. This is the default.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// Stop executing further checks on the first failure.
    /// </summary>
    StopOnFirstFailure = 1
}
