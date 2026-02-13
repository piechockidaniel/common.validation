using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atlas.Common.Validation.Json.Models;

/// <summary>
/// Defines a single validation rule within a <see cref="PropertyDefinition"/>.
/// </summary>
public sealed class RuleDefinition
{
    /// <summary>
    /// Gets or sets the validator type name (e.g. "notEmpty", "maxLength", "email").
    /// </summary>
    [JsonPropertyName(name: "validator")]
    public string Validator { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional parameters for the validator (e.g. { "max": 100 }).
    /// </summary>
    [JsonPropertyName(name: "params")]
    public JsonElement? Params { get; set; }

    /// <summary>
    /// Gets or sets the error message to display when validation fails.
    /// </summary>
    [JsonPropertyName(name: "message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional error code for programmatic handling.
    /// </summary>
    [JsonPropertyName(name: "errorCode")]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the default severity level as a string (e.g. "forbidden", "atOwnRisk", "notRecommended").
    /// </summary>
    [JsonPropertyName(name: "severity")]
    public string? Severity { get; set; }

    /// <summary>
    /// Gets or sets layer-specific severity overrides.
    /// Keys are layer names, values are severity level strings.
    /// </summary>
    [JsonPropertyName(name: "layers")]
    public IDictionary<string, string>? Layers { get; set; }
}
