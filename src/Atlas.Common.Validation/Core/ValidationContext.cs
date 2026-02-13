namespace Atlas.Common.Validation.Core;

/// <summary>
/// Default implementation of <see cref="IValidationContext"/>.
/// </summary>
public class ValidationContext(string? layer = null, IDictionary<string, object?>? items = null) : IValidationContext
{
    /// <inheritdoc />
    public string? Layer { get; } = layer;

    /// <inheritdoc />
    public IDictionary<string, object?>? Items { get; set; } = items;
    /// <summary>
    /// Creates a context for a specific layer.
    /// </summary>
    /// <param name="layer">The validation layer name.</param>
    /// <returns>A new <see cref="ValidationContext"/>.</returns>
    public static ValidationContext ForLayer(string layer) => new(layer: layer);
}
