using System.Reflection;
using Common.Validation.Core;
using Common.Validation.Json;
using Common.Validation.Json.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Common.Validation.DependencyInjection;

/// <summary>
/// Extension methods for registering Common.Validation services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Common.Validation core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCommonValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configure = null)
    {
        var options = new ValidationOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IValidatorTypeRegistry, ValidatorTypeRegistry>();
        services.TryAddSingleton<JsonValidationDefinitionLoader>();
        services.TryAddTransient<IValidatorFactory, DefaultValidatorFactory>();

        // Load JSON definitions if configured
        if (options.JsonDefinitionPaths.Count > 0)
        {
            var loader = new JsonValidationDefinitionLoader();
            foreach (var path in options.JsonDefinitionPaths)
            {
                if (File.Exists(path))
                {
                    var definition = loader.LoadFromFile(path);
                    services.AddSingleton(definition);
                }
                else if (Directory.Exists(path))
                {
                    var definitions = loader.LoadFromDirectory(path);
                    foreach (var definition in definitions)
                    {
                        services.AddSingleton(definition);
                    }
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Scans the specified assembly for types implementing <see cref="IValidator{T}"/>
    /// and registers them in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="lifetime">The service lifetime. Default is <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var validatorTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .Where(t => t.GetInterfaces().Any(IsValidatorInterface));

        foreach (var validatorType in validatorTypes)
        {
            var interfaceTypes = validatorType.GetInterfaces().Where(IsValidatorInterface);

            foreach (var interfaceType in interfaceTypes)
            {
                var descriptor = new ServiceDescriptor(interfaceType, validatorType, lifetime);
                services.TryAdd(descriptor);
            }

            // Also register as non-generic IValidator
            services.TryAdd(new ServiceDescriptor(typeof(IValidator), validatorType, lifetime));
        }

        return services;
    }

    /// <summary>
    /// Scans the assembly containing <typeparamref name="TMarker"/> for validators
    /// and registers them in the service collection.
    /// </summary>
    /// <typeparam name="TMarker">A type from the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddValidatorsFromAssemblyContaining<TMarker>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddValidatorsFromAssembly(typeof(TMarker).Assembly, lifetime);
    }

    private static bool IsValidatorInterface(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>);
    }
}
