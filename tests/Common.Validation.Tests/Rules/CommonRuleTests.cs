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
        var v = CreateValidator(b => b.NotNull().WithMessage("Required"));
        var result = v.Validate(new Model { Text = null });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void NotNull_Passes_WhenNotNull()
    {
        var v = CreateValidator(b => b.NotNull());
        Assert.True(v.Validate(new Model { Text = "hello" }).IsValid);
    }

    [Fact]
    public void Null_Fails_WhenNotNull()
    {
        var v = CreateValidator(b => b.Null());
        Assert.False(v.Validate(new Model { Text = "hello" }).IsValid);
    }

    [Fact]
    public void Null_Passes_WhenNull()
    {
        var v = CreateValidator(b => b.Null());
        Assert.True(v.Validate(new Model { Text = null }).IsValid);
    }

    [Fact]
    public void NotEmpty_Fails_WhenEmptyString()
    {
        var v = CreateValidator(b => b.NotEmpty());
        Assert.False(v.Validate(new Model { Text = "" }).IsValid);
    }

    [Fact]
    public void NotEmpty_Fails_WhenWhitespace()
    {
        var v = CreateValidator(b => b.NotEmpty());
        Assert.False(v.Validate(new Model { Text = "   " }).IsValid);
    }

    [Fact]
    public void NotEmpty_Passes_WhenHasValue()
    {
        var v = CreateValidator(b => b.NotEmpty());
        Assert.True(v.Validate(new Model { Text = "hello" }).IsValid);
    }

    [Fact]
    public void Equal_Passes_WhenEqual()
    {
        var v = CreateValidator(b => b.Equal("expected"));
        Assert.True(v.Validate(new Model { Text = "expected" }).IsValid);
    }

    [Fact]
    public void Equal_Fails_WhenNotEqual()
    {
        var v = CreateValidator(b => b.Equal("expected"));
        Assert.False(v.Validate(new Model { Text = "other" }).IsValid);
    }

    [Fact]
    public void NotEqual_Passes_WhenDifferent()
    {
        var v = CreateValidator(b => b.NotEqual("forbidden"));
        Assert.True(v.Validate(new Model { Text = "allowed" }).IsValid);
    }

    [Fact]
    public void Must_CustomPredicate_Works()
    {
        var v = CreateValidator(b => b.Must(s => s != null && s.StartsWith("A"), "Must start with A"));
        Assert.True(v.Validate(new Model { Text = "Alice" }).IsValid);
        Assert.False(v.Validate(new Model { Text = "Bob" }).IsValid);
    }

    [Fact]
    public void Must_InstancePredicate_Works()
    {
        var v = new InstancePredicateValidator();
        Assert.True(v.Validate(new Model { Text = "hello", Number = 5 }).IsValid);
        Assert.False(v.Validate(new Model { Text = "hi", Number = 5 }).IsValid);
    }

    private class InstancePredicateValidator : AbstractValidator<Model>
    {
        public InstancePredicateValidator()
        {
            RuleFor(x => x.Text)
                .Must((instance, text) => text != null && text.Length >= instance.Number,
                    "Text must be at least as long as Number.");
        }
    }

    private static IValidator<Model> CreateValidator(
        Action<Common.Validation.Rules.IRuleBuilder<Model, string?>> configure)
    {
        return new InlineValidator(configure);
    }

    private class InlineValidator : AbstractValidator<Model>
    {
        public InlineValidator(Action<Common.Validation.Rules.IRuleBuilder<Model, string?>> configure)
        {
            var builder = RuleFor(x => x.Text);
            configure(builder);
        }
    }
}
