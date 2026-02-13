using System.Text.Json.Serialization;

namespace Atlas.Common.Validation.Json.Models;

/// <summary>
/// Root definition of a JSON-based validation schema for a specific type.
/// </summary>
public sealed class ValidationDefinition
{
    /// <summary>
    /// Gets or sets the optional JSON Schema reference for editor support.
    /// </summary>
    [JsonPropertyName(name: "$schema")]
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets the type name this definition applies to.
    /// </summary>
    [JsonPropertyName(name: "type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property validation definitions keyed by property name.
    /// </summary>
    [JsonPropertyName(name: "properties")]
    public IDictionary<string, PropertyDefinition>? Properties { get; set; }
}
