using System.Text.Json.Serialization;

namespace Common.Validation.Json.Models;

/// <summary>
/// Defines validation rules for a single property within a <see cref="ValidationDefinition"/>.
/// </summary>
public sealed class PropertyDefinition
{
    /// <summary>
    /// Gets or sets the list of validation rules for this property.
    /// </summary>
    [JsonPropertyName("rules")]
    public List<RuleDefinition> Rules { get; set; } = new();
}
