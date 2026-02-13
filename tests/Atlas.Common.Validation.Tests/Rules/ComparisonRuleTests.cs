using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Extensions;

namespace Atlas.Common.Validation.Tests.Rules;

public class ComparisonRuleTests
{
    private class Model
    {
        public int Value { get; set; }
    }

    [Theory]
    [InlineData(data: [5, 3, true])]
    [InlineData(data: [3, 3, false])]
    [InlineData(data: [1, 3, false])]
    public void GreaterThan_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(configure: b => b.GreaterThan(threshold: threshold));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: [5, 3, true])]
    [InlineData(data: [3, 3, true])]
    [InlineData(data: [1, 3, false])]
    public void GreaterThanOrEqual_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(configure: b => b.GreaterThanOrEqual(threshold: threshold));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: [1, 3, true])]
    [InlineData(data: [3, 3, false])]
    [InlineData(data: [5, 3, false])]
    public void LessThan_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(configure: b => b.LessThan(threshold: threshold));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: [1, 3, true])]
    [InlineData(data: [3, 3, true])]
    [InlineData(data: [5, 3, false])]
    public void LessThanOrEqual_ValidatesCorrectly(int value, int threshold, bool expected)
    {
        var v = Create(configure: b => b.LessThanOrEqual(threshold: threshold));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    [Theory]
    [InlineData(data: [5, 1, 10, true])]
    [InlineData(data: [1, 1, 10, true])]
    [InlineData(data: [10, 1, 10, true])]
    [InlineData(data: [0, 1, 10, false])]
    [InlineData(data: [11, 1, 10, false])]
    public void InclusiveBetween_ValidatesCorrectly(int value, int from, int to, bool expected)
    {
        var v = Create(configure: b => b.InclusiveBetween(from: from, to: to));
        Assert.Equal(expected: expected, actual: v.Validate(instance: new Model { Value = value }).IsValid);
    }

    private static IValidator<Model> Create(
        Action<Common.Validation.Rules.IRuleBuilder<Model, int>> configure)
    {
        return new InlineValidator(configure: configure);
    }

    private class InlineValidator : AbstractValidator<Model>
    {
        public InlineValidator(Action<Common.Validation.Rules.IRuleBuilder<Model, int>> configure)
        {
            var builder = RuleFor(expression: x => x.Value);
            configure(obj: builder);
        }
    }
}
