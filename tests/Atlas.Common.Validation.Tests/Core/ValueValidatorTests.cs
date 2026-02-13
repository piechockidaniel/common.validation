using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Extensions;

namespace Atlas.Common.Validation.Tests.Core;

public class ValueValidatorTests
{
    #region Class-based validator

    private class EmailValidator : ValueValidator<string>
    {
        public EmailValidator() : base(defaultPropertyName: "Email")
        {
            Check()
                .NotEmpty().WithMessage(message: "Email is required.")
                .EmailAddress().WithMessage(message: "Must be a valid email address.")
                .MaxLength(max: 255).WithMessage(message: "Email is too long.");
        }
    }

    private class AgeValidator : ValueValidator<int>
    {
        public AgeValidator() : base(defaultPropertyName: "Age")
        {
            Check()
                .GreaterThanOrEqual(threshold: 0).WithMessage(message: "Age cannot be negative.")
                .LessThanOrEqual(threshold: 150).WithMessage(message: "Age is unrealistic.");
        }
    }

    [Fact]
    public void ClassBased_ValidValue_ReturnsSuccess()
    {
        var validator = new EmailValidator();
        var result = validator.Validate(value: "user@example.com");

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void ClassBased_EmptyValue_ReturnsFailure()
    {
        var validator = new EmailValidator();
        var result = validator.Validate(value: "");

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => string.Equals(a: e.ErrorMessage, b: "Email is required.", comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    public void ClassBased_InvalidEmail_ReturnsFailure()
    {
        var validator = new EmailValidator();
        var result = validator.Validate(value: "not-an-email");

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => string.Equals(a: e.ErrorMessage, b: "Must be a valid email address.", comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    public void ClassBased_IntValidator_ValidValue_ReturnsSuccess()
    {
        var validator = new AgeValidator();
        var result = validator.Validate(value: 25);

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void ClassBased_IntValidator_NegativeValue_ReturnsFailure()
    {
        var validator = new AgeValidator();
        var result = validator.Validate(value: -1);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => string.Equals(a: e.ErrorMessage, b: "Age cannot be negative.", comparisonType: StringComparison.Ordinal));
    }

    #endregion

    #region Inline factory

    [Fact]
    public void InlineCreate_ValidValue_ReturnsSuccess()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty().MinLength(min: 3),
            propertyName: "Name");

        var result = validator.Validate(value: "Alice");

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void InlineCreate_InvalidValue_ReturnsFailure()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty().MinLength(min: 3),
            propertyName: "Name");

        var result = validator.Validate(value: "Al");

        Assert.False(condition: result.IsValid);
    }

    [Fact]
    public void InlineCreate_NullConfigure_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(testCode: () =>
            ValueValidator.Create<string>(configure: null!));
    }

    #endregion

    #region Non-generic interface

    [Fact]
    public void NonGeneric_Validate_WithCorrectType_Works()
    {
        IValueValidator validator = new EmailValidator();
        var result = validator.Validate(value: "user@example.com");

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void NonGeneric_Validate_WithWrongType_ThrowsArgumentException()
    {
        IValueValidator validator = new EmailValidator();
        Assert.Throws<ArgumentException>(testCode: () => validator.Validate(value: 42));
    }

    [Fact]
    public void NonGeneric_Validate_WithNull_ValidatesNull()
    {
        IValueValidator validator = new EmailValidator();
        var result = validator.Validate(value: null);

        Assert.False(condition: result.IsValid); // NotEmpty should fail
    }

    [Fact]
    public void NonGeneric_ValidatedType_ReturnsCorrectType()
    {
        IValueValidator validator = new EmailValidator();
        Assert.Equal(expected: typeof(string), actual: validator.ValidatedType);
    }

    [Fact]
    public void NonGeneric_ValidateWithContext_Works()
    {
        IValueValidator validator = new EmailValidator();
        var context = ValidationContext.ForLayer(layer: "api");
        var result = validator.Validate(value: "user@example.com", context: context);

        Assert.True(condition: result.IsValid);
    }

    #endregion

    #region Layer support

    private class LayerAwareValidator : ValueValidator<string>
    {
        public LayerAwareValidator() : base(defaultPropertyName: "Field")
        {
            Check()
                .NotEmpty()
                .WithSeverity(severity: Severity.Forbidden)
                .WithLayerSeverity(layer: "api", severity: Severity.NotRecommended)
                .WithLayerSeverity(layer: "entity", severity: Severity.Forbidden);
        }
    }

    [Fact]
    public void LayerContext_OverridesSeverity()
    {
        var validator = new LayerAwareValidator();
        var apiContext = ValidationContext.ForLayer(layer: "api");

        var result = validator.Validate(value: "", context: apiContext);

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: Severity.NotRecommended, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void NoLayerContext_UsesDefaultSeverity()
    {
        var validator = new LayerAwareValidator();

        var result = validator.Validate(value: "");

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: Severity.Forbidden, actual: result.Errors[index: 0].Severity);
    }

    #endregion

    #region Cascade mode

    private class CascadeStopValidator : ValueValidator<string>
    {
        public CascadeStopValidator() : base(defaultPropertyName: "Field")
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Check().NotEmpty().WithMessage(message: "First rule failed.");
            Check().MinLength(min: 10).WithMessage(message: "Second rule failed.");
        }
    }

    [Fact]
    public void CascadeMode_StopOnFirstFailure_StopsAfterFirstRule()
    {
        var validator = new CascadeStopValidator();
        var result = validator.Validate(value: "");

        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "First rule failed.", actual: result.Errors[index: 0].ErrorMessage);
    }

    #endregion

    #region Multiple rules (Check() called multiple times)

    private class MultiRuleValidator : ValueValidator<string>
    {
        public MultiRuleValidator() : base(defaultPropertyName: "Value")
        {
            Check().NotNull().WithMessage(message: "Value cannot be null.");
            Check().NotEmpty().WithMessage(message: "Value cannot be empty.");
            Check().MinLength(min: 3).WithMessage(message: "Value is too short.");
        }
    }

    [Fact]
    public void MultipleRules_AllFail_ReturnsAllFailures()
    {
        var validator = new MultiRuleValidator();
        var result = validator.Validate(value: null!);

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: 3, actual: result.Errors.Count);
    }

    [Fact]
    public void MultipleRules_SomeFail_ReturnsOnlyFailedRules()
    {
        var validator = new MultiRuleValidator();
        var result = validator.Validate(value: "ab");

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "Value is too short.", actual: result.Errors[index: 0].ErrorMessage);
    }

    [Fact]
    public void MultipleRules_AllPass_ReturnsValid()
    {
        var validator = new MultiRuleValidator();
        var result = validator.Validate(value: "hello");

        Assert.True(condition: result.IsValid);
    }

    #endregion

    #region NullContext guard

    [Fact]
    public void Validate_NullContext_ThrowsArgumentNullException()
    {
        var validator = new EmailValidator();
        Assert.Throws<ArgumentNullException>(testCode: () =>
            validator.Validate(value: "test", context: null!));
    }

    #endregion

    #region Reusability — same validator, different values

    [Fact]
    public void SameValidator_DifferentValues_ProducesIndependentResults()
    {
        var validator = new EmailValidator();

        var valid = validator.Validate(value: "alice@example.com");
        var invalid = validator.Validate(value: "not-email");

        Assert.True(condition: valid.IsValid);
        Assert.False(condition: invalid.IsValid);
    }

    #endregion
}
