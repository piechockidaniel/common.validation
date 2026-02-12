using Common.Validation.Core;
using Common.Validation.Extensions;
using Common.Validation.Rules;

namespace Common.Validation.Tests.Rules;

public class ValueRuleTests
{
    #region Common rules

    [Fact]
    public void NotNull_Null_ReturnsFalse()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotNull(),
            propertyName: "Field");

        var result = validator.Validate(value: null!);

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "Field", actual: result.Errors[index: 0].PropertyName);
    }

    [Fact]
    public void NotNull_NonNull_ReturnsTrue()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotNull(),
            propertyName: "Field");

        var result = validator.Validate(value: "hello");

        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(data: [null, false])]
    [InlineData(data: ["", false])]
    [InlineData(data: ["  ", false])]
    [InlineData(data: ["hello", true])]
    public void NotEmpty_String_ValidatesCorrectly(string? value, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty(),
            propertyName: "Field");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value!).IsValid);
    }

    [Theory]
    [InlineData(data: [null, true])]
    [InlineData(data: ["", true])]
    [InlineData(data: ["  ", true])]
    [InlineData(data: ["hello", false])]
    public void Empty_String_ValidatesCorrectly(string? value, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.Empty(),
            propertyName: "Field");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value!).IsValid);
    }

    [Fact]
    public void Equal_MatchingValue_ReturnsTrue()
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.Equal(comparisonValue: 42),
            propertyName: "Age");

        Assert.True(condition: validator.Validate(value: 42).IsValid);
    }

    [Fact]
    public void Equal_DifferentValue_ReturnsFalse()
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.Equal(comparisonValue: 42),
            propertyName: "Age");

        Assert.False(condition: validator.Validate(value: 99).IsValid);
    }

    [Fact]
    public void NotEqual_DifferentValue_ReturnsTrue()
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.NotEqual(comparisonValue: 0),
            propertyName: "Count");

        Assert.True(condition: validator.Validate(value: 5).IsValid);
    }

    [Fact]
    public void Must_CustomPredicate_Works()
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.Must(predicate: v => v % 2 == 0, message: "must be even."),
            propertyName: "Number");

        Assert.True(condition: validator.Validate(value: 4).IsValid);
        Assert.False(condition: validator.Validate(value: 3).IsValid);
    }

    #endregion

    #region String rules

    [Theory]
    [InlineData(data: ["ab", 5, false])]
    [InlineData(data: ["abcde", 5, true])]
    [InlineData(data: ["abcdef", 5, true])]
    public void MinLength_ValidatesCorrectly(string value, int min, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.MinLength(min: min),
            propertyName: "Text");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: ["abc", 5, true])]
    [InlineData(data: ["abcde", 5, true])]
    [InlineData(data: ["abcdef", 5, false])]
    public void MaxLength_ValidatesCorrectly(string value, int max, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.MaxLength(max: max),
            propertyName: "Text");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: ["ab", 3, 5, false])]
    [InlineData(data: ["abc", 3, 5, true])]
    [InlineData(data: ["abcde", 3, 5, true])]
    [InlineData(data: ["abcdef", 3, 5, false])]
    public void Length_ValidatesRange(string value, int min, int max, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.Length(min: min, max: max),
            propertyName: "Text");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: ["user@example.com", true])]
    [InlineData(data: ["user@sub.example.com", true])]
    [InlineData(data: ["not-an-email", false])]
    [InlineData(data: ["@missing.com", false])]
    public void EmailAddress_ValidatesCorrectly(string value, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.EmailAddress(),
            propertyName: "Email");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: ["+48 123 456 789", true])]
    [InlineData(data: ["123-456-7890", true])]
    [InlineData(data: ["abc", false])]
    [InlineData(data: ["12", false])]
    public void PhoneNumber_ValidatesCorrectly(string value, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.PhoneNumber(),
            propertyName: "Phone");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: ["ABC123", @"^[A-Z]+\d+$", true])]
    [InlineData(data: ["abc123", @"^[A-Z]+\d+$", false])]
    public void Matches_Pattern_ValidatesCorrectly(string value, string pattern, bool expected)
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.Matches(pattern: pattern),
            propertyName: "Code");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    #endregion

    #region Comparison rules

    [Theory]
    [InlineData(data: [5, 3, true])]
    [InlineData(data: [3, 3, false])]
    [InlineData(data: [1, 3, false])]
    public void GreaterThan_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.GreaterThan(threshold: threshold),
            propertyName: "Amount");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: [5, 3, true])]
    [InlineData(data: [3, 3, true])]
    [InlineData(data: [1, 3, false])]
    public void GreaterThanOrEqual_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.GreaterThanOrEqual(threshold: threshold),
            propertyName: "Amount");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: [1, 3, true])]
    [InlineData(data: [3, 3, false])]
    [InlineData(data: [5, 3, false])]
    public void LessThan_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.LessThan(threshold: threshold),
            propertyName: "Amount");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: [1, 3, true])]
    [InlineData(data: [3, 3, true])]
    [InlineData(data: [5, 3, false])]
    public void LessThanOrEqual_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.LessThanOrEqual(threshold: threshold),
            propertyName: "Amount");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    [Theory]
    [InlineData(data: [0, 1, 10, false])]
    [InlineData(data: [1, 1, 10, true])]
    [InlineData(data: [5, 1, 10, true])]
    [InlineData(data: [10, 1, 10, true])]
    [InlineData(data: [11, 1, 10, false])]
    public void InclusiveBetween_ValidatesCorrectly(int value, int from, int to, bool expected)
    {
        var validator = ValueValidator.Create<int>(
            configure: b => b.InclusiveBetween(from: from, to: to),
            propertyName: "Score");

        Assert.Equal(expected: expected, actual: validator.Validate(value: value).IsValid);
    }

    #endregion

    #region Severity and layers

    [Fact]
    public void WithSeverity_SetsCorrectSeverity()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty().WithSeverity(severity: Severity.AtOwnRisk),
            propertyName: "Name");

        var result = validator.Validate(value: "");

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: Severity.AtOwnRisk, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void WithLayerSeverity_OverridesSeverityForLayer()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .NotEmpty()
                .WithSeverity(severity: Severity.Forbidden)
                .WithLayerSeverity(layer: "api", severity: Severity.NotRecommended),
            propertyName: "Name");

        var apiContext = ValidationContext.ForLayer(layer: "api");
        var result = validator.Validate(value: "", context: apiContext);

        Assert.Equal(expected: Severity.NotRecommended, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void DefaultSeverity_IsForbidden()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty(),
            propertyName: "Name");

        var result = validator.Validate(value: "");

        Assert.Equal(expected: Severity.Forbidden, actual: result.Errors[index: 0].Severity);
    }

    #endregion

    #region Messages and error codes

    [Fact]
    public void WithMessage_SetsCustomMessage()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty().WithMessage(message: "Please provide a value."),
            propertyName: "Name");

        var result = validator.Validate(value: "");

        Assert.Equal(expected: "Please provide a value.", actual: result.Errors[index: 0].ErrorMessage);
    }

    [Fact]
    public void WithErrorCode_SetsErrorCode()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty().WithErrorCode(errorCode: "REQUIRED"),
            propertyName: "Name");

        var result = validator.Validate(value: "");

        Assert.Equal(expected: "REQUIRED", actual: result.Errors[index: 0].ErrorCode);
    }

    #endregion

    #region Conditions

    [Fact]
    public void When_ConditionTrue_CheckExecutes()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .When(condition: v => v is not null)
                .MinLength(min: 3),
            propertyName: "Code");

        var result = validator.Validate(value: "ab");

        Assert.False(condition: result.IsValid);
    }

    [Fact]
    public void When_ConditionFalse_CheckSkipped()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .When(condition: v => v is not null)
                .MinLength(min: 3),
            propertyName: "Code");

        var result = validator.Validate(value: null!);

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void Unless_ConditionTrue_CheckSkipped()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .Unless(condition: v => v is null)
                .MinLength(min: 3),
            propertyName: "Code");

        var result = validator.Validate(value: null!);

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void Unless_ConditionFalse_CheckExecutes()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .Unless(condition: v => v is null)
                .MinLength(min: 3),
            propertyName: "Code");

        var result = validator.Validate(value: "ab");

        Assert.False(condition: result.IsValid);
    }

    #endregion

    #region Cascade mode

    [Fact]
    public void Cascade_StopOnFirstFailure_StopsAfterFirst()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .Cascade(cascadeMode: CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .MinLength(min: 5),
            propertyName: "Text");

        var result = validator.Validate(value: "");

        Assert.Single(collection: result.Errors);
    }

    [Fact]
    public void Cascade_Continue_ReturnsAllFailures()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .NotEmpty()
                .MinLength(min: 5),
            propertyName: "Text");

        var result = validator.Validate(value: "");

        Assert.Equal(expected: 2, actual: result.Errors.Count);
    }

    #endregion

    #region Multiple checks chain

    [Fact]
    public void ChainedChecks_AllPass_ReturnsValid()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .NotNull()
                .NotEmpty()
                .MinLength(min: 2)
                .MaxLength(max: 50)
                .EmailAddress(),
            propertyName: "Email");

        var result = validator.Validate(value: "user@example.com");

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void ChainedChecks_SomeFail_ReturnsAllFailures()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b
                .NotEmpty()
                .MinLength(min: 10)
                .EmailAddress(),
            propertyName: "Email");

        var result = validator.Validate(value: "ab");

        Assert.Equal(expected: 2, actual: result.Errors.Count); // MinLength + EmailAddress
    }

    #endregion

    #region PropertyName

    [Fact]
    public void PropertyName_DefaultsToTypeName()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty());

        var result = validator.Validate(value: "");

        Assert.Equal(expected: "String", actual: result.Errors[index: 0].PropertyName);
    }

    [Fact]
    public void PropertyName_CanBeCustomized()
    {
        var validator = ValueValidator.Create<string>(
            configure: b => b.NotEmpty(),
            propertyName: "EmailAddress");

        var result = validator.Validate(value: "");

        Assert.Equal(expected: "EmailAddress", actual: result.Errors[index: 0].PropertyName);
    }

    #endregion
}
