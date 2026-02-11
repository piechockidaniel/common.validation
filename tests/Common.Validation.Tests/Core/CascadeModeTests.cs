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
        var result = validator.Validate(instance: new TestModel { Value = "" });

        // Should only get the NotEmpty failure, not the MinLength
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "Must not be empty.", actual: result.Errors[index: 0].ErrorMessage);
    }

    [Fact]
    public void PropertyCascade_Continue_ReportsAllFailures()
    {
        var validator = new PropertyContinueValidator();
        var result = validator.Validate(instance: new TestModel { Value = "" });

        Assert.Equal(expected: 2, actual: result.Errors.Count);
    }

    private class PropertyCascadeValidator : AbstractValidator<TestModel>
    {
        public PropertyCascadeValidator()
        {
            RuleFor(expression: x => x.Value)
                .Cascade(cascadeMode: CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage(message: "Must not be empty.")
                .MinLength(min: 5).WithMessage(message: "Must be at least 5 chars.");
        }
    }

    private class PropertyContinueValidator : AbstractValidator<TestModel>
    {
        public PropertyContinueValidator()
        {
            RuleFor(expression: x => x.Value)
                .NotEmpty().WithMessage(message: "Must not be empty.")
                .MinLength(min: 5).WithMessage(message: "Must be at least 5 chars.");
        }
    }
}
