using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.DependencyInjection;
using Atlas.Common.Validation.Extensions;
using Atlas.Common.Validation.Json.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Common.Validation.Tests.DependencyInjection;

public class ServiceCollectionTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Required");
        }
    }

    [Fact]
    public void AddCommonValidation_RegistersCoreServices()
    {
        var services = new ServiceCollection();
        services.AddCommonValidation();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(@object: provider.GetService<ValidationOptions>());
        Assert.NotNull(@object: provider.GetService<IValidatorTypeRegistry>());
        Assert.NotNull(@object: provider.GetService<IValidatorFactory>());
    }

    [Fact]
    public void AddValidatorsFromAssembly_RegistersValidators()
    {
        var services = new ServiceCollection();
        services.AddCommonValidation();
        services.AddValidatorsFromAssembly(assembly: typeof(ServiceCollectionTests).Assembly);

        var provider = services.BuildServiceProvider();

        var validator = provider.GetService<IValidator<TestModel>>();
        Assert.NotNull(@object: validator);
        Assert.IsType<TestValidator>(@object: validator);
    }

    [Fact]
    public void AddValidatorsFromAssemblyContaining_RegistersValidators()
    {
        var services = new ServiceCollection();
        services.AddCommonValidation();
        services.AddValidatorsFromAssemblyContaining<ServiceCollectionTests>();

        var provider = services.BuildServiceProvider();

        var validator = provider.GetService<IValidator<TestModel>>();
        Assert.NotNull(@object: validator);
    }

    [Fact]
    public void ValidatorFactory_ResolvesValidator()
    {
        var services = new ServiceCollection();
        services.AddCommonValidation();
        services.AddValidatorsFromAssembly(assembly: typeof(ServiceCollectionTests).Assembly);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IValidatorFactory>();

        var validator = factory.GetValidator<TestModel>();
        Assert.NotNull(@object: validator);

        var result = validator
            !.Validate(instance: new TestModel { Name = "" });
        Assert.False(condition: result.IsValid);
    }

    [Fact]
    public void ValidatorFactory_GetValidatorByType_Works()
    {
        var services = new ServiceCollection();
        services.AddCommonValidation();
        services.AddValidatorsFromAssembly(assembly: typeof(ServiceCollectionTests).Assembly);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IValidatorFactory>();

        var validator = factory.GetValidator<TestModel>();
        Assert.NotNull(@object: validator);
        Assert.Equal(expected: typeof(TestModel), actual: validator!.ValidatedType);
    }

    [Fact]
    public void ValidatorFactory_ReturnsNull_WhenNotRegistered()
    {
        var services = new ServiceCollection();
        services.AddCommonValidation();

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IValidatorFactory>();

        var validator = factory.GetValidator<string>();
        Assert.Null(@object: validator);
    }
}
