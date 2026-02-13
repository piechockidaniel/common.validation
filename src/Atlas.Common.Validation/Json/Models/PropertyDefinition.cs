using System.Text.Json.Serialization;

namespace Atlas.Common.Validation.Json.Models;

/// <summary>
/// Defines validation rules for a single property within a <see cref="ValidationDefinition"/>.
/// </summary>
public sealed class PropertyDefinition
{
    /// <summary>
    /// Gets or sets the list of validation rules for this property.
    /// </summary>
    [JsonPropertyName(name: "rules")]
    public IList<RuleDefinition>? Rules { get; set; }
}
