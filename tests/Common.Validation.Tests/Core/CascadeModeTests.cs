using Common.Validation.Core;
using Common.Validation.Extensions;

namespace Common.Validation.Tests.Core;

public class CascadeModeTests
{
    private class TestModel
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void PropertyCascade_StopOnFirstFailure_StopsAfterFirstCheck()
    {
        var validator = new PropertyCascadeValidator();
        var result = validator.Validate(new TestModel { Value = "" });

        // Should only get the NotEmpty failure, not the MinLength
        Assert.Single(result.Errors);
        Assert.Equal("Must not be empty.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void PropertyCascade_Continue_ReportsAllFailures()
    {
        var validator = new PropertyContinueValidator();
        var result = validator.Validate(new TestModel { Value = "" });

        Assert.Equal(2, result.Errors.Count);
    }

    private class PropertyCascadeValidator : AbstractValidator<TestModel>
    {
        public PropertyCascadeValidator()
        {
            RuleFor(x => x.Value)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("Must not be empty.")
                .MinLength(5).WithMessage("Must be at least 5 chars.");
        }
    }

    private class PropertyContinueValidator : AbstractValidator<TestModel>
    {
        public PropertyContinueValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Must not be empty.")
                .MinLength(5).WithMessage("Must be at least 5 chars.");
        }
    }
}
