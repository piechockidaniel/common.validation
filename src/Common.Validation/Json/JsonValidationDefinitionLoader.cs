using System.Text.Json;
using Common.Validation.Json.Models;

namespace Common.Validation.Json;

/// <summary>
/// Loads <see cref="ValidationDefinition"/> instances from JSON sources.
/// </summary>
public static class JsonValidationDefinitionLoader
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Loads a validation definition from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing the validation definition.</param>
    /// <returns>A parsed <see cref="ValidationDefinition"/>.</returns>
    public static ValidationDefinition Load(this string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<ValidationDefinition>(json, DefaultOptions)
               ?? throw new JsonException("Failed to deserialize validation definition: result was null.");
    }

    /// <summary>
    /// Loads a validation definition from a stream.
    /// </summary>
    /// <param name="stream">The stream containing JSON data.</param>
    /// <returns>A parsed <see cref="ValidationDefinition"/>.</returns>
    public static ValidationDefinition Load(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return JsonSerializer.Deserialize<ValidationDefinition>(stream, DefaultOptions)
               ?? throw new JsonException("Failed to deserialize validation definition: result was null.");
    }

    /// <summary>
    /// Loads a validation definition from a file path.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>A parsed <see cref="ValidationDefinition"/>.</returns>
    public static ValidationDefinition LoadFromFile(this string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        using var stream = File.OpenRead(filePath);
        return Load(stream);
    }

    /// <summary>
    /// Loads all validation definitions from JSON files in a directory.
    /// </summary>
    /// <param name="directoryPath">The directory containing JSON definition files.</param>
    /// <param name="searchPattern">The file search pattern. Default is "*.validation.json".</param>
    /// <returns>A list of parsed <see cref="ValidationDefinition"/> instances.</returns>
    public static IReadOnlyList<ValidationDefinition> LoadFromDirectory(
        this string directoryPath, string searchPattern = "*.validation.json")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Validation definitions directory not found: {directoryPath}");

        var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        var definitions = new List<ValidationDefinition>(files.Length);

        foreach (var file in files)
        {
            definitions.Add(file.LoadFromFile());
        }

        return definitions.AsReadOnly();
    }
}
