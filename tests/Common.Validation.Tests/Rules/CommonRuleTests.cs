using Common.Validation.Core;
using Common.Validation.Extensions;

namespace Common.Validation.Tests.Rules;

public class CommonRuleTests
{
    private class Model
    {
        public string? Text { get; set; }
        public int Number { get; set; }
        public List<string>? Items { get; set; }
    }

    [Fact]
    public void NotNull_Fails_WhenNull()
    {
        var v = CreateValidator(configure: b => b.NotNull().WithMessage(message: "Required"));
        var result = v.Validate(instance: new Model { Text = null });
        Assert.False(condition: result.IsValid);
    }

    [Fact]
    public void NotNull_Passes_WhenNotNull()
    {
        var v = CreateValidator(configure: b => b.NotNull());
        Assert.True(condition: v.Validate(instance: new Model { Text = "hello" }).IsValid);
    }

    [Fact]
    public void Null_Fails_WhenNotNull()
    {
        var v = CreateValidator(configure: b => b.Null());
        Assert.False(condition: v.Validate(instance: new Model { Text = "hello" }).IsValid);
    }

    [Fact]
    public void Null_Passes_WhenNull()
    {
        var v = CreateValidator(configure: b => b.Null());
        Assert.True(condition: v.Validate(instance: new Model { Text = null }).IsValid);
    }

    [Fact]
    public void NotEmpty_Fails_WhenEmptyString()
    {
        var v = CreateValidator(configure: b => b.NotEmpty());
        Assert.False(condition: v.Validate(instance: new Model { Text = "" }).IsValid);
    }

    [Fact]
    public void NotEmpty_Fails_WhenWhitespace()
    {
        var v = CreateValidator(configure: b => b.NotEmpty());
        Assert.False(condition: v.Validate(instance: new Model { Text = "   " }).IsValid);
    }

    [Fact]
    public void NotEmpty_Passes_WhenHasValue()
    {
        var v = CreateValidator(configure: b => b.NotEmpty());
        Assert.True(condition: v.Validate(instance: new Model { Text = "hello" }).IsValid);
    }

    [Fact]
    public void Equal_Passes_WhenEqual()
    {
        var v = CreateValidator(configure: b => b.Equal(comparisonValue: "expected"));
        Assert.True(condition: v.Validate(instance: new Model { Text = "expected" }).IsValid);
    }

    [Fact]
    public void Equal_Fails_WhenNotEqual()
    {
        var v = CreateValidator(configure: b => b.Equal(comparisonValue: "expected"));
        Assert.False(condition: v.Validate(instance: new Model { Text = "other" }).IsValid);
    }

    [Fact]
    public void NotEqual_Passes_WhenDifferent()
    {
        var v = CreateValidator(configure: b => b.NotEqual(comparisonValue: "forbidden"));
        Assert.True(condition: v.Validate(instance: new Model { Text = "allowed" }).IsValid);
    }

    [Fact]
    public void Must_CustomPredicate_Works()
    {
        var v = CreateValidator(configure: b => b.Must(predicate: s => s?.StartsWith(value: 'A') ?? false, message: "Must start with A"));
        Assert.True(condition: v.Validate(instance: new Model { Text = "Alice" }).IsValid);
        Assert.False(condition: v.Validate(instance: new Model { Text = "Bob" }).IsValid);
    }

    [Fact]
    public void Must_InstancePredicate_Works()
    {
        var v = new InstancePredicateValidator();
        Assert.True(condition: v.Validate(instance: new Model { Text = "hello", Number = 5 }).IsValid);
        Assert.False(condition: v.Validate(instance: new Model { Text = "hi", Number = 5 }).IsValid);
    }

    private class InstancePredicateValidator : AbstractValidator<Model>
    {
        public InstancePredicateValidator()
        {
            RuleFor(expression: x => x.Text)
                .Must(predicate: (instance, text) => text != null && text.Length >= instance.Number,
                    message: "Text must be at least as long as Number.");
        }
    }

    private static InlineValidator CreateValidator(
        Action<Common.Validation.Rules.IRuleBuilder<Model, string?>> configure)
    {
        return new InlineValidator(configure: configure);
    }

    private class InlineValidator : AbstractValidator<Model>
    {
        public InlineValidator(Action<Common.Validation.Rules.IRuleBuilder<Model, string?>> configure)
        {
            var builder = RuleFor(expression: x => x.Text);
            configure(obj: builder);
        }
    }
}
