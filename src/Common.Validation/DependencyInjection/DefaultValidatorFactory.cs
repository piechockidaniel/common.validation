using Common.Validation.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Validation.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IValidatorFactory"/> that resolves validators from an <see cref="IServiceProvider"/>.
/// </summary>
internal sealed class DefaultValidatorFactory : IValidatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultValidatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IValidator<T>? GetValidator<T>()
    {
        return _serviceProvider.GetService<IValidator<T>>();
    }

    /// <inheritdoc />
    public IValidator? GetValidator(Type type)
    {
        var genericType = typeof(IValidator<>).MakeGenericType(typeArguments: type);
        return _serviceProvider.GetService(serviceType: genericType) as IValidator;
    }
}
