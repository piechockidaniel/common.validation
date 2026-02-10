using Common.Validation.Core;
using Common.Validation.Extensions;

namespace Common.Validation.Tests.Rules;

public class StringRuleTests
{
    private class Model
    {
        public string Value { get; set; } = string.Empty;
    }

    [Theory]
    [InlineData("ab", 5, false)]
    [InlineData("abcde", 5, true)]
    [InlineData("abcdef", 5, true)]
    public void MinLength_ValidatesCorrectly(string value, int min, bool expected)
    {
        var v = Create(b => b.MinLength(min));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData("abc", 5, true)]
    [InlineData("abcde", 5, true)]
    [InlineData("abcdef", 5, false)]
    public void MaxLength_ValidatesCorrectly(string value, int max, bool expected)
    {
        var v = Create(b => b.MaxLength(max));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData("ab", 3, 5, false)]
    [InlineData("abc", 3, 5, true)]
    [InlineData("abcde", 3, 5, true)]
    [InlineData("abcdef", 3, 5, false)]
    public void Length_ValidatesRange(string value, int min, int max, bool expected)
    {
        var v = Create(b => b.Length(min, max));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user@sub.example.com", true)]
    [InlineData("not-an-email", false)]
    [InlineData("@missing.com", false)]
    [InlineData("missing@.com", false)]
    public void EmailAddress_ValidatesCorrectly(string value, bool expected)
    {
        var v = Create(b => b.EmailAddress());
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData("+48 123 456 789", true)]
    [InlineData("123-456-7890", true)]
    [InlineData("(123) 456 7890", true)]
    [InlineData("abc", false)]
    [InlineData("12", false)]
    public void PhoneNumber_ValidatesCorrectly(string value, bool expected)
    {
        var v = Create(b => b.PhoneNumber());
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData("ABC123", @"^[A-Z]+\d+$", true)]
    [InlineData("abc123", @"^[A-Z]+\d+$", false)]
    public void Matches_Pattern_ValidatesCorrectly(string value, string pattern, bool expected)
    {
        var v = Create(b => b.Matches(pattern));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    private static IValidator<Model> Create(
        Action<Common.Validation.Rules.IRuleBuilder<Model, string>> configure)
    {
        return new InlineValidator(configure);
    }

    private class InlineValidator : AbstractValidator<Model>
    {
        public InlineValidator(Action<Common.Validation.Rules.IRuleBuilder<Model, string>> configure)
        {
            var builder = RuleFor(x => x.Value);
            configure(builder);
        }
    }
}
