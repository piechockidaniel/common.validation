using Atlas.Common.Validation.Core;

namespace Atlas.Common.Validation.DependencyInjection;

/// <summary>
/// Configuration options for the Atlas.Common.Validation framework.
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Gets or sets the default cascade mode for all validators.
    /// Default is <see cref="Core.CascadeMode.Continue"/>.
    /// </summary>
    public CascadeMode DefaultCascadeMode { get; set; } = CascadeMode.Continue;

    /// <summary>
    /// Gets or sets the default validation layer to use when none is specified.
    /// When <c>null</c>, layer resolution falls back to the <c>[ValidationLayer]</c> attribute on the validated type.
    /// </summary>
    public string? DefaultLayer { get; set; }

    /// <summary>
    /// Gets or sets paths to directories or files containing JSON validation definitions.
    /// Definitions will be loaded and registered as validators automatically.
    /// </summary>
    public IList<string>? JsonDefinitionPaths { get; set; }
}
