using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Extensions;

namespace Atlas.Common.Validation.Tests.Rules;

public class StringRuleTests
{
    private class Model
    {
        public string Value { get; set; } = string.Empty;
    }

    [Theory]
    [InlineData(data: ["ab", 5, false])]
    [InlineData(data: ["abcde", 5, true])]
    [InlineData(data: ["abcdef", 5, true])]
    public void MinLength_ValidatesCorrectly(string value, int min, bool expected)
    {
        var v = Create(configure: b => b.MinLength(min: min));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: ["abc", 5, true])]
    [InlineData(data: ["abcde", 5, true])]
    [InlineData(data: ["abcdef", 5, false])]
    public void MaxLength_ValidatesCorrectly(string value, int max, bool expected)
    {
        var v = Create(configure: b => b.MaxLength(max: max));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: ["ab", 3, 5, false])]
    [InlineData(data: ["abc", 3, 5, true])]
    [InlineData(data: ["abcde", 3, 5, true])]
    [InlineData(data: ["abcdef", 3, 5, false])]
    public void Length_ValidatesRange(string value, int min, int max, bool expected)
    {
        var v = Create(configure: b => b.Length(min: min, max: max));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: ["user@example.com", true])]
    [InlineData(data: ["user@sub.example.com", true])]
    [InlineData(data: ["not-an-email", false])]
    [InlineData(data: ["@missing.com", false])]
    [InlineData(data: ["missing@.com", false])]
    public void EmailAddress_ValidatesCorrectly(string value, bool expected)
    {
        var v = Create(configure: b => b.EmailAddress());
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: ["+48 123 456 789", true])]
    [InlineData(data: ["123-456-7890", true])]
    [InlineData(data: ["(123) 456 7890", true])]
    [InlineData(data: ["abc", false])]
    [InlineData(data: ["12", false])]
    public void PhoneNumber_ValidatesCorrectly(string value, bool expected)
    {
        var v = Create(configure: b => b.PhoneNumber());
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: ["ABC123", @"^[A-Z]+\d+$", true])]
    [InlineData(data: ["abc123", @"^[A-Z]+\d+$", false])]
    public void Matches_Pattern_ValidatesCorrectly(string value, string pattern, bool expected)
    {
        var v = Create(configure: b => b.Matches(pattern: pattern));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    private static IValidator<Model> Create(
        Action<Common.Validation.Rules.IRuleBuilder<Model, string>> configure)
    {
        return new InlineValidator(configure: configure);
    }

    private class InlineValidator : AbstractValidator<Model>
    {
        public InlineValidator(Action<Common.Validation.Rules.IRuleBuilder<Model, string>> configure)
        {
            var builder = RuleFor(expression: x => x.Value);
            configure(obj: builder);
        }
    }
}
