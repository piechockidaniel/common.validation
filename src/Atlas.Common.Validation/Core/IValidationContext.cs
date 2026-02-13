namespace Atlas.Common.Validation.Core;

/// <summary>
/// Provides contextual information for validation operations,
/// including the active validation layer and custom data.
/// </summary>
public interface IValidationContext
{
    /// <summary>
    /// Gets the active validation layer (e.g. "api", "dto", "entity").
    /// When <c>null</c>, the default severity is used for all rules.
    /// </summary>
    string? Layer { get; }

    /// <summary>
    /// Gets a dictionary for passing custom data through the validation pipeline.
    /// </summary>
    IDictionary<string, object?>? Items { get; }
}
