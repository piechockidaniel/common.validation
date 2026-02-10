using Common.Validation.Json.Registry;

namespace Common.Validation.Tests.Json;

public class ValidatorTypeRegistryTests
{
    [Theory]
    [InlineData("notNull")]
    [InlineData("null")]
    [InlineData("notEmpty")]
    [InlineData("empty")]
    [InlineData("maxLength")]
    [InlineData("minLength")]
    [InlineData("length")]
    [InlineData("email")]
    [InlineData("phone")]
    [InlineData("matches")]
    [InlineData("equal")]
    [InlineData("notEqual")]
    [InlineData("greaterThan")]
    [InlineData("greaterThanOrEqual")]
    [InlineData("lessThan")]
    [InlineData("lessThanOrEqual")]
    [InlineData("inclusiveBetween")]
    public void BuiltInValidators_AreRegistered(string name)
    {
        var registry = new ValidatorTypeRegistry();
        Assert.True(registry.IsRegistered(name));
    }

    [Fact]
    public void Resolve_UnregisteredValidator_ThrowsInvalidOperation()
    {
        var registry = new ValidatorTypeRegistry();
        Assert.Throws<InvalidOperationException>(() => registry.Resolve("nonexistent", null));
    }

    [Fact]
    public void Register_CustomValidator_Works()
    {
        var registry = new ValidatorTypeRegistry();
        registry.Register("custom", _ => new TestCheck(v => v is string s && s == "custom"));

        var check = registry.Resolve("custom", null);
        Assert.True(check.IsValid("custom"));
        Assert.False(check.IsValid("other"));
    }

    [Fact]
    public void Register_CaseInsensitive()
    {
        var registry = new ValidatorTypeRegistry();
        Assert.True(registry.IsRegistered("NotEmpty"));
        Assert.True(registry.IsRegistered("NOTEMPTY"));
        Assert.True(registry.IsRegistered("notempty"));
    }

    private class TestCheck : IPropertyCheck
    {
        private readonly Func<object?, bool> _predicate;
        public TestCheck(Func<object?, bool> predicate) => _predicate = predicate;
        public bool IsValid(object? value) => _predicate(value);
    }
}
