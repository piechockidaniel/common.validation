using Atlas.Common.Validation.Json.Registry;

namespace Atlas.Common.Validation.Tests.Json;

public class ValidatorTypeRegistryTests
{
    [Theory]
    [InlineData(data: "notNull")]
    [InlineData(data: "null")]
    [InlineData(data: "notEmpty")]
    [InlineData(data: "empty")]
    [InlineData(data: "maxLength")]
    [InlineData(data: "minLength")]
    [InlineData(data: "length")]
    [InlineData(data: "email")]
    [InlineData(data: "phone")]
    [InlineData(data: "matches")]
    [InlineData(data: "equal")]
    [InlineData(data: "notEqual")]
    [InlineData(data: "greaterThan")]
    [InlineData(data: "greaterThanOrEqual")]
    [InlineData(data: "lessThan")]
    [InlineData(data: "lessThanOrEqual")]
    [InlineData(data: "inclusiveBetween")]
    public void BuiltInValidators_AreRegistered(string name)
    {
        var registry = new ValidatorTypeRegistry();
        Assert.True(condition: registry.IsRegistered(name: name));
    }

    [Fact]
    public void Resolve_UnregisteredValidator_ThrowsInvalidOperation()
    {
        var registry = new ValidatorTypeRegistry();
        Assert.Throws<InvalidOperationException>(testCode: () => registry.Resolve(name: "nonexistent",
            parameters: null));
    }

    [Fact]
    public void Register_CustomValidator_Works()
    {
        var registry = new ValidatorTypeRegistry();
        registry.Register(name: "custom", factory: _ => new TestCheck(predicate: v => v is string s && s.Equals("custom", StringComparison.InvariantCultureIgnoreCase)));

        var check = registry.Resolve(name: "custom", parameters: null);
        Assert.True(condition: check.IsValid(value: "custom"));
        Assert.False(condition: check.IsValid(value: "other"));
    }

    [Fact]
    public void Register_CaseInsensitive()
    {
        var registry = new ValidatorTypeRegistry();
        Assert.True(condition: registry.IsRegistered(name: "NotEmpty"));
        Assert.True(condition: registry.IsRegistered(name: "NOTEMPTY"));
        Assert.True(condition: registry.IsRegistered(name: "notempty"));
    }

    private class TestCheck(Func<object?, bool> predicate) : IPropertyCheck
    {
        public bool IsValid(object? value) => predicate(arg: value);
    }
}
