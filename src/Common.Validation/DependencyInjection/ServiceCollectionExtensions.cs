using System.Reflection;
using Common.Validation.Core;
using Common.Validation.Json;
using Common.Validation.Json.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Loader = Common.Validation.Json.JsonValidationDefinitionLoader;

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
        configure?.Invoke(obj: options);

        services.TryAddSingleton(instance: options);
        services.TryAddSingleton<IValidatorTypeRegistry, ValidatorTypeRegistry>();
        services.TryAddTransient<IValidatorFactory, DefaultValidatorFactory>();


        if (options.JsonDefinitionPaths?.Count > 0)
        {
            foreach (var path in options.JsonDefinitionPaths)
            {
                if (File.Exists(path: path))
                {
                    var definition = path.LoadFromFile();
                    services.AddSingleton(implementationInstance: definition);
                }
                else if (Directory.Exists(path: path))
                {
                    var definitions = path.LoadFromDirectory();
                    foreach (var definition in definitions)
                    {
                        services.AddSingleton(implementationInstance: definition);
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
        ArgumentNullException.ThrowIfNull(argument: assembly);

        var validatorTypes = assembly.GetTypes()
            .Where(predicate: t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
                                   && t.GetInterfaces().Any(predicate: IsValidatorInterface));

        foreach (var validatorType in validatorTypes)
        {
            var interfaceTypes = validatorType.GetInterfaces().Where(predicate: IsValidatorInterface);

            foreach (var interfaceType in interfaceTypes)
            {
                var descriptor = new ServiceDescriptor(serviceType: interfaceType, implementationType: validatorType, lifetime: lifetime);
                services.TryAdd(descriptor: descriptor);
            }

            // Also register as non-generic IValidator
            services.TryAdd(descriptor: new ServiceDescriptor(serviceType: typeof(IValidator), implementationType: validatorType, lifetime: lifetime));
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
        return services.AddValidatorsFromAssembly(assembly: typeof(TMarker).Assembly, lifetime: lifetime);
    }

    private static bool IsValidatorInterface(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>);
    }
}
