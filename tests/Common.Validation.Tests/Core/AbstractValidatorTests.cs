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
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.").WithSeverity(Severity.Forbidden);

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Age must be positive.").WithSeverity(Severity.Forbidden);
        }
    }

    [Fact]
    public void Validate_ValidModel_ReturnsSuccess()
    {
        var validator = new TestValidator();
        var result = validator.Validate(new TestModel { Name = "Alice", Age = 30 });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_InvalidModel_ReturnsFailures()
    {
        var validator = new TestValidator();
        var result = validator.Validate(new TestModel { Name = "", Age = 0 });

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Validate_NullInstance_ThrowsArgumentNullException()
    {
        var validator = new TestValidator();
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public void Validate_NonGeneric_WorksCorrectly()
    {
        IValidator validator = new TestValidator();
        var result = validator.Validate(new TestModel { Name = "Alice", Age = 30 });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NonGeneric_WrongType_ThrowsArgumentException()
    {
        IValidator validator = new TestValidator();
        Assert.Throws<ArgumentException>(() => validator.Validate("wrong type"));
    }

    [Fact]
    public void ValidatedType_ReturnsCorrectType()
    {
        IValidator validator = new TestValidator();
        Assert.Equal(typeof(TestModel), validator.ValidatedType);
    }

    [Fact]
    public void CascadeMode_StopOnFirstFailure_StopsAfterFirstRule()
    {
        var validator = new CascadeStopValidator();
        var result = validator.Validate(new TestModel { Name = "", Age = 0 });

        // Should stop after the first rule (Name) produces a failure
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    private class CascadeStopValidator : AbstractValidator<TestModel>
    {
        public CascadeStopValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Age must be positive.");
        }
    }
}
