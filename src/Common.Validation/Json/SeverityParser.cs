using Common.Validation.Core;

namespace Common.Validation.Json;

/// <summary>
/// Parses severity level strings from JSON definitions to <see cref="Severity"/> enum values.
/// </summary>
internal static class SeverityParser
{
    /// <summary>
    /// Parses a string severity value to <see cref="Severity"/>.
    /// </summary>
    /// <param name="value">The severity string (case-insensitive).</param>
    /// <param name="defaultSeverity">Default if the string is null or empty.</param>
    /// <returns>The parsed <see cref="Severity"/> value.</returns>
    public static Severity Parse(string? value, Severity defaultSeverity = Severity.Forbidden)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultSeverity;

        return value.Trim().ToLowerInvariant() switch
        {
            "forbidden" => Severity.Forbidden,
            "atownrisk" or "at_own_risk" or "atown_risk" => Severity.AtOwnRisk,
            "notrecommended" or "not_recommended" => Severity.NotRecommended,
            _ => throw new ArgumentException($"Unknown severity value: '{value}'. Expected: 'forbidden', 'atOwnRisk', or 'notRecommended'.")
        };
    }
}
