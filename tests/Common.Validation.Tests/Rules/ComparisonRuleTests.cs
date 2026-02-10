using Common.Validation.Core;
using Common.Validation.Extensions;

namespace Common.Validation.Tests.Rules;

public class ComparisonRuleTests
{
    private class Model
    {
        public int Value { get; set; }
    }

    [Theory]
    [InlineData(5, 3, true)]
    [InlineData(3, 3, false)]
    [InlineData(1, 3, false)]
    public void GreaterThan_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(b => b.GreaterThan(threshold));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(5, 3, true)]
    [InlineData(3, 3, true)]
    [InlineData(1, 3, false)]
    public void GreaterThanOrEqual_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(b => b.GreaterThanOrEqual(threshold));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(1, 3, true)]
    [InlineData(3, 3, false)]
    [InlineData(5, 3, false)]
    public void LessThan_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(b => b.LessThan(threshold));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(1, 3, true)]
    [InlineData(3, 3, true)]
    [InlineData(5, 3, false)]
    public void LessThanOrEqual_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(b => b.LessThanOrEqual(threshold));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, true)]
    [InlineData(10, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    public void InclusiveBetween_ValidatesCorrectly(int value, int from, int to, bool expected)
    {
        var v = Create(b => b.InclusiveBetween(from, to));
        Assert.Equal(expected, v.Validate(new Model { Value = value }).IsValid);
    }

    private static IValidator<Model> Create(
        Action<Common.Validation.Rules.IRuleBuilder<Model, int>> configure)
    {
        return new InlineValidator(configure);
    }

    private class InlineValidator : AbstractValidator<Model>
    {
        public InlineValidator(Action<Common.Validation.Rules.IRuleBuilder<Model, int>> configure)
        {
            var builder = RuleFor(x => x.Value);
            configure(builder);
        }
    }
}
