using Common.Validation.Core;
using Common.Validation.Extensions;
using Common.Validation.Layers;

namespace Common.Validation.Tests.Layers;

public class LayerTests
{
    private class BaseModel
    {
        public string Name { get; set; } = string.Empty;
    }

    [ValidationLayer(layer: "api")]
    private class ApiModel : BaseModel;

    [ValidationLayer(layer: "entity")]
    private class EntityModel : BaseModel;

    private class ApiValidator : AbstractValidator<ApiModel>
    {
        public ApiValidator()
        {
            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Name is required.")
                .WithSeverity(severity: Severity.AtOwnRisk)
                .WithLayerSeverity(layer: "api", severity: Severity.Forbidden)
                .WithLayerSeverity(layer: "entity", severity: Severity.NotRecommended);
        }
    }

    private class EntityValidator : AbstractValidator<EntityModel>
    {
        public EntityValidator()
        {
            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Name is required.")
                .WithSeverity(severity: Severity.AtOwnRisk)
                .WithLayerSeverity(layer: "api", severity: Severity.Forbidden)
                .WithLayerSeverity(layer: "entity", severity: Severity.NotRecommended);
        }
    }

    [Fact]
    public void ApiModel_UsesApiLayerSeverity_Automatically()
    {
        var validator = new ApiValidator();
        var result = validator.Validate(instance: new ApiModel { Name = "" });

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: Severity.Forbidden, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void EntityModel_UsesEntityLayerSeverity_Automatically()
    {
        var validator = new EntityValidator();
        var result = validator.Validate(instance: new EntityModel { Name = "" });

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: Severity.NotRecommended, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void ExplicitContext_OverridesAttributeLayer()
    {
        var validator = new ApiValidator();
        var context = ValidationContext.ForLayer(layer: "entity");
        var result = validator.Validate(instance: new ApiModel { Name = "" }, context: context);

        // Even though ApiModel has [ValidationLayer("api")], we explicitly pass "entity"
        Assert.Equal(expected: Severity.NotRecommended, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void NoLayer_UsesDefaultSeverity()
    {
        var validator = new NoLayerValidator();
        var result = validator.Validate(instance: new BaseModel { Name = "" });

        // BaseModel has no [ValidationLayer] attribute, so default AtOwnRisk applies
        Assert.Equal(expected: Severity.AtOwnRisk, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void UnknownLayer_UsesDefaultSeverity()
    {
        var validator = new NoLayerValidator();
        var context = ValidationContext.ForLayer(layer: "unknown");
        var result = validator.Validate(instance: new BaseModel { Name = "" }, context: context);

        // "unknown" is not in the LayerSeverities map, so default applies
        Assert.Equal(expected: Severity.AtOwnRisk, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void ValidationLayerAttribute_ReturnsCorrectLayer()
    {
        var attr = typeof(ApiModel).GetCustomAttributes(attributeType: typeof(ValidationLayerAttribute), inherit: true)
            .Cast<ValidationLayerAttribute>()
            .FirstOrDefault();

        Assert.NotNull(@object: attr);
        Assert.Equal(expected: "api", actual: attr.Layer);
    }

    // --- Inheritance scenario: attribute on base, child has no attribute ---

    [ValidationLayer(layer: "api")]
    private class ApiBaseModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private class InheritedApiModel : ApiBaseModel;

    private class InheritedApiValidator : AbstractValidator<InheritedApiModel>
    {
        public InheritedApiValidator()
        {
            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Name is required.")
                .WithSeverity(severity: Severity.AtOwnRisk)
                .WithLayerSeverity(layer: "api", severity: Severity.Forbidden);
        }
    }

    [Fact]
    public void InheritedLayer_ResolvesFromBaseClass()
    {
        // InheritedApiModel has no [ValidationLayer] itself, but inherits from ApiBaseModel which has [ValidationLayer("api")]
        var validator = new InheritedApiValidator();
        var result = validator.Validate(instance: new InheritedApiModel { Name = "" });

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: Severity.Forbidden, actual: result.Errors[index: 0].Severity);
    }

    private class NoLayerValidator : AbstractValidator<BaseModel>
    {
        public NoLayerValidator()
        {
            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Name is required.")
                .WithSeverity(severity: Severity.AtOwnRisk)
                .WithLayerSeverity(layer: "api", severity: Severity.Forbidden)
                .WithLayerSeverity(layer: "entity", severity: Severity.NotRecommended);
        }
    }
}
