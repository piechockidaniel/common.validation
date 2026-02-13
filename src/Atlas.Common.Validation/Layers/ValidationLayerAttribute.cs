namespace Atlas.Common.Validation.Layers;

/// <summary>
/// Marks a class with its validation layer identity.
/// When a type decorated with this attribute is validated,
/// the validator will automatically resolve layer-specific severity overrides.
/// </summary>
/// <remarks>
/// <para>
/// Common layer names include "api", "dto", and "entity",
/// but any string value is accepted for extensibility.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// [ValidationLayer("api")]
/// public class PersonalDataApiModel { ... }
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
public sealed class ValidationLayerAttribute : Attribute
{
    /// <summary>
    /// Gets the layer name assigned to the decorated type.
    /// </summary>
    public string Layer { get; }

    /// <summary>
    /// Creates a new <see cref="ValidationLayerAttribute"/> with the specified layer name.
    /// </summary>
    /// <param name="layer">The validation layer name (e.g. "api", "dto", "entity").</param>
    public ValidationLayerAttribute(string layer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: layer);
        Layer = layer;
    }
}
