namespace Common.Validation.Core;

/// <summary>
/// Default implementation of <see cref="IValidationContext"/>.
/// </summary>
public class ValidationContext : IValidationContext
{
    /// <inheritdoc />
    public string? Layer { get; }

    /// <inheritdoc />
    public IDictionary<string, object?> Items { get; }

    /// <summary>
    /// Creates a new <see cref="ValidationContext"/> with optional layer and items.
    /// </summary>
    /// <param name="layer">The active validation layer.</param>
    /// <param name="items">Optional custom data dictionary.</param>
    public ValidationContext(string? layer = null, IDictionary<string, object?>? items = null)
    {
        Layer = layer;
        Items = items ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Creates a context for a specific layer.
    /// </summary>
    /// <param name="layer">The validation layer name.</param>
    /// <returns>A new <see cref="ValidationContext"/>.</returns>
    public static ValidationContext ForLayer(string layer) => new(layer);
}
