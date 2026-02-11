using Common.Validation.Core;
using Common.Validation.Extensions;

namespace Common.Validation.Tests.Core;

public class AbstractValidatorTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Name is required.").WithSeverity(severity: Severity.Forbidden);

            RuleFor(expression: x => x.Age)
                .GreaterThan(threshold: 0).WithMessage(message: "Age must be positive.").WithSeverity(severity: Severity.Forbidden);
        }
    }

    [Fact]
    public void Validate_ValidModel_ReturnsSuccess()
    {
        var validator = new TestValidator();
        var result = validator.Validate(instance: new TestModel { Name = "Alice", Age = 30 });

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void Validate_InvalidModel_ReturnsFailures()
    {
        var validator = new TestValidator();
        var result = validator.Validate(instance: new TestModel { Name = "", Age = 0 });

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: 2, actual: result.Errors.Count);
    }

    [Fact]
    public void Validate_NullInstance_ThrowsArgumentNullException()
    {
        var validator = new TestValidator();
        Assert.Throws<ArgumentNullException>(testCode: () => validator.Validate(instance: null!));
    }

    [Fact]
    public void Validate_NonGeneric_WorksCorrectly()
    {
        IValidator validator = new TestValidator();
        var result = validator.Validate(instance: new TestModel { Name = "Alice", Age = 30 });
        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void Validate_NonGeneric_WrongType_ThrowsArgumentException()
    {
        IValidator validator = new TestValidator();
        Assert.Throws<ArgumentException>(testCode: () => validator.Validate(instance: "wrong type"));
    }

    [Fact]
    public void ValidatedType_ReturnsCorrectType()
    {
        IValidator validator = new TestValidator();
        Assert.Equal(expected: typeof(TestModel), actual: validator.ValidatedType);
    }

    [Fact]
    public void CascadeMode_StopOnFirstFailure_StopsAfterFirstRule()
    {
        var validator = new CascadeStopValidator();
        var result = validator.Validate(instance: new TestModel { Name = "", Age = 0 });

        // Should stop after the first rule (Name) produces a failure
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "Name", actual: result.Errors[index: 0].PropertyName);
    }

    [Fact]
    public void ValidateProperty_ValidProperty_ReturnsSuccess()
    {
        var validator = new TestValidator();
        var model = new TestModel { Name = "Alice", Age = 0 }; // Age invalid, Name valid

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.Name);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void ValidateProperty_InvalidProperty_ReturnsOnlyThatPropertyFailures()
    {
        var validator = new TestValidator();
        var model = new TestModel { Name = "", Age = 0 };

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.Name);

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "Name", actual: result.Errors[index: 0].PropertyName);
    }

    [Fact]
    public void ValidateProperty_DoesNotValidateOtherProperties()
    {
        var validator = new TestValidator();
        var model = new TestModel { Name = "Alice", Age = -5 }; // Age invalid

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.Name);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void ValidateProperty_WithContext_RespectsLayer()
    {
        var validator = new TestValidator();
        var model = new TestModel { Name = "", Age = 30 };
        var context = ValidationContext.ForLayer(layer: "entity");

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.Name, context: context);

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
    }

    [Fact]
    public void ValidateProperty_NullInstance_ThrowsArgumentNullException()
    {
        var validator = new TestValidator();
        Assert.Throws<ArgumentNullException>(testCode: () =>
            validator.ValidateProperty(instance: null!, propertyExpression: (TestModel x) => x.Name));
    }

    private class CascadeStopValidator : AbstractValidator<TestModel>
    {
        public CascadeStopValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(expression: x => x.Name)
                .NotEmpty().WithMessage(message: "Name is required.");

            RuleFor(expression: x => x.Age)
                .GreaterThan(threshold: 0).WithMessage(message: "Age must be positive.");
        }
    }
}
