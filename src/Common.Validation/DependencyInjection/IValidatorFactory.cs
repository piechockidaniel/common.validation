using Common.Validation.Core;

namespace Common.Validation.DependencyInjection;

/// <summary>
/// Factory for resolving validators at runtime.
/// Supports both fluent (code-based) and JSON-based validators.
/// </summary>
public interface IValidatorFactory
{
    /// <summary>
    /// Gets a validator for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <returns>A validator instance, or <c>null</c> if no validator is registered for this type.</returns>
    IValidator<T>? GetValidator<T>();

    /// <summary>
    /// Gets a validator for the specified type.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <returns>A validator instance, or <c>null</c> if no validator is registered for this type.</returns>
    IValidator? GetValidator(Type type);
}
