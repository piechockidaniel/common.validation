using System.Text.Json;

namespace Atlas.Common.Validation.Json.Registry;

/// <summary>
/// Registry that maps validator type names (e.g. "notEmpty", "maxLength")
/// to factory functions that create <see cref="IPropertyCheck"/> instances.
/// </summary>
public interface IValidatorTypeRegistry
{
    /// <summary>
    /// Registers a factory for a validator type.
    /// </summary>
    /// <param name="name">The validator type name (case-insensitive).</param>
    /// <param name="factory">A factory that takes optional JSON parameters and returns a check.</param>
    void Register(string name, Func<JsonElement?, IPropertyCheck> factory);

    /// <summary>
    /// Resolves a validator type name to an <see cref="IPropertyCheck"/> instance.
    /// </summary>
    /// <param name="name">The validator type name.</param>
    /// <param name="parameters">Optional JSON parameters for the check.</param>
    /// <returns>An <see cref="IPropertyCheck"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the validator type is not registered.</exception>
    IPropertyCheck Resolve(string name, JsonElement? parameters);

    /// <summary>
    /// Gets whether a validator type is registered.
    /// </summary>
    /// <param name="name">The validator type name.</param>
    /// <returns><c>true</c> if registered; otherwise, <c>false</c>.</returns>
    bool IsRegistered(string name);
}
