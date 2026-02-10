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

    [ValidationLayer("api")]
    private class ApiModel : BaseModel { }

    [ValidationLayer("entity")]
    private class EntityModel : BaseModel { }

    private class ApiValidator : AbstractValidator<ApiModel>
    {
        public ApiValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .WithSeverity(Severity.AtOwnRisk)
                .WithLayerSeverity("api", Severity.Forbidden)
                .WithLayerSeverity("entity", Severity.NotRecommended);
        }
    }

    private class EntityValidator : AbstractValidator<EntityModel>
    {
        public EntityValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .WithSeverity(Severity.AtOwnRisk)
                .WithLayerSeverity("api", Severity.Forbidden)
                .WithLayerSeverity("entity", Severity.NotRecommended);
        }
    }

    [Fact]
    public void ApiModel_UsesApiLayerSeverity_Automatically()
    {
        var validator = new ApiValidator();
        var result = validator.Validate(new ApiModel { Name = "" });

        Assert.False(result.IsValid);
        Assert.Equal(Severity.Forbidden, result.Errors[0].Severity);
    }

    [Fact]
    public void EntityModel_UsesEntityLayerSeverity_Automatically()
    {
        var validator = new EntityValidator();
        var result = validator.Validate(new EntityModel { Name = "" });

        Assert.False(result.IsValid);
        Assert.Equal(Severity.NotRecommended, result.Errors[0].Severity);
    }

    [Fact]
    public void ExplicitContext_OverridesAttributeLayer()
    {
        var validator = new ApiValidator();
        var context = ValidationContext.ForLayer("entity");
        var result = validator.Validate(new ApiModel { Name = "" }, context);

        // Even though ApiModel has [ValidationLayer("api")], we explicitly pass "entity"
        Assert.Equal(Severity.NotRecommended, result.Errors[0].Severity);
    }

    [Fact]
    public void NoLayer_UsesDefaultSeverity()
    {
        var validator = new NoLayerValidator();
        var result = validator.Validate(new BaseModel { Name = "" });

        // BaseModel has no [ValidationLayer] attribute, so default AtOwnRisk applies
        Assert.Equal(Severity.AtOwnRisk, result.Errors[0].Severity);
    }

    [Fact]
    public void UnknownLayer_UsesDefaultSeverity()
    {
        var validator = new NoLayerValidator();
        var context = ValidationContext.ForLayer("unknown");
        var result = validator.Validate(new BaseModel { Name = "" }, context);

        // "unknown" is not in the LayerSeverities map, so default applies
        Assert.Equal(Severity.AtOwnRisk, result.Errors[0].Severity);
    }

    [Fact]
    public void ValidationLayerAttribute_ReturnsCorrectLayer()
    {
        var attr = typeof(ApiModel).GetCustomAttributes(typeof(ValidationLayerAttribute), true)
            .Cast<ValidationLayerAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attr);
        Assert.Equal("api", attr.Layer);
    }

    // --- Inheritance scenario: attribute on base, child has no attribute ---

    [ValidationLayer("api")]
    private class ApiBaseModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private class InheritedApiModel : ApiBaseModel { }

    private class InheritedApiValidator : AbstractValidator<InheritedApiModel>
    {
        public InheritedApiValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .WithSeverity(Severity.AtOwnRisk)
                .WithLayerSeverity("api", Severity.Forbidden);
        }
    }

    [Fact]
    public void InheritedLayer_ResolvesFromBaseClass()
    {
        // InheritedApiModel has no [ValidationLayer] itself, but inherits from ApiBaseModel which has [ValidationLayer("api")]
        var validator = new InheritedApiValidator();
        var result = validator.Validate(new InheritedApiModel { Name = "" });

        Assert.False(result.IsValid);
        Assert.Equal(Severity.Forbidden, result.Errors[0].Severity);
    }

    private class NoLayerValidator : AbstractValidator<BaseModel>
    {
        public NoLayerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .WithSeverity(Severity.AtOwnRisk)
                .WithLayerSeverity("api", Severity.Forbidden)
                .WithLayerSeverity("entity", Severity.NotRecommended);
        }
    }
}
