using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Extensions;
using Atlas.Common.Validation.Json;

namespace Atlas.Common.Validation.Tests.Json;

public class JsonValidatorTests
{
    private class TestModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private const string TestJson = """
    {
        "type": "TestModel",
        "properties": {
            "firstName": {
                "rules": [
                    {
                        "validator": "notEmpty",
                        "message": "First name is required.",
                        "severity": "forbidden",
                        "layers": {
                            "api": "forbidden",
                            "entity": "notRecommended"
                        }
                    },
                    {
                        "validator": "maxLength",
                        "params": { "max": 50 },
                        "message": "First name too long."
                    }
                ]
            },
            "email": {
                "rules": [
                    {
                        "validator": "email",
                        "message": "Invalid email.",
                        "severity": "forbidden"
                    }
                ]
            },
            "age": {
                "rules": [
                    {
                        "validator": "greaterThan",
                        "params": { "value": 0 },
                        "message": "Age must be positive."
                    }
                ]
            }
        }
    }
    """;

    [Fact]
    public void JsonValidator_ValidModel_ReturnsSuccess()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);

        var result = validator.Validate(instance: new TestModel
        {
            FirstName = "Alice",
            Email = "alice@example.com",
            Age = 25
        });

        Assert.True(condition: result.IsValid);
    }

    [Fact]
    public void JsonValidator_InvalidModel_ReturnsFailures()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);

        var result = validator.Validate(instance: new TestModel
        {
            FirstName = "",
            Email = "not-email",
            Age = 0
        });

        Assert.False(condition: result.IsValid);
        Assert.Equal(expected: 3, actual: result.Errors.Count);
    }

    [Fact]
    public void JsonValidator_RespectsSeverity()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);

        var result = validator.Validate(instance: new TestModel
        {
            FirstName = "",
            Email = "valid@test.com",
            Age = 25
        });

        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: Severity.Forbidden, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void JsonValidator_LayerSeverity_Api()
    {

        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);

        var context = ValidationContext.ForLayer(layer: "api");
        var result = validator.Validate(instance: new TestModel { FirstName = "", Email = "a@b.c", Age = 1 }, context: context);

        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: Severity.Forbidden, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void JsonValidator_LayerSeverity_Entity()
    {

        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);

        var context = ValidationContext.ForLayer(layer: "entity");
        var result = validator.Validate(instance: new TestModel { FirstName = "", Email = "a@b.c", Age = 1 }, context: context);

        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: Severity.NotRecommended, actual: result.Errors[index: 0].Severity);
    }

    [Fact]
    public void JsonValidationDefinitionLoader_LoadsDefinition()
    {

        var definition = TestJson.Load();

        Assert.Equal(expected: "TestModel", actual: definition.Type);
        Assert.Equal(expected: 3, actual: definition.Properties?.Count);
        Assert.True(condition: definition.Properties?.ContainsKey(key: "firstName"));
        Assert.True(condition: definition.Properties?.ContainsKey(key: "email"));
        Assert.True(condition: definition.Properties?.ContainsKey(key: "age"));
    }

    [Fact]
    public void JsonValidator_MaxLength_FailsWhenTooLong()
    {

        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);

        var result = validator.Validate(instance: new TestModel
        {
            FirstName = new string(c: 'A', count: 51),
            Email = "a@b.c",
            Age = 1
        });

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => string.Equals(a: e.ErrorMessage, b: "First name too long.", comparisonType: StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public void JsonValidator_UnknownProperty_ThrowsInvalidOperation()
    {
        var json = """
        {
            "type": "TestModel",
            "properties": {
                "nonExistent": {
                    "rules": [
                        { "validator": "notEmpty", "message": "err" }
                    ]
                }
            }
        }
        """;

        var definition = json.Load();
        Assert.Throws<InvalidOperationException>(testCode: () => new JsonValidator<TestModel>(definition: definition));
    }

    [Fact]
    public void JsonValidator_UnknownValidator_ThrowsInvalidOperation()
    {
        var json = """
        {
            "type": "TestModel",
            "properties": {
                "firstName": {
                    "rules": [
                        { "validator": "doesNotExist", "message": "err" }
                    ]
                }
            }
        }
        """;


        var definition = json.Load();

        Assert.Throws<InvalidOperationException>(testCode: () => new JsonValidator<TestModel>(definition: definition));
    }

    [Fact]
    public void JsonValidator_ValidateProperty_ValidProperty_ReturnsSuccess()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);
        var model = new TestModel { FirstName = "Alice", Email = "not-email", Age = 0 }; // Email and Age invalid

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.FirstName);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void JsonValidator_ValidateProperty_InvalidProperty_ReturnsOnlyThatPropertyFailures()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);
        var model = new TestModel { FirstName = "", Email = "alice@example.com", Age = 25 };

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.FirstName);

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: "FirstName", actual: result.Errors[index: 0].PropertyName);
    }

    [Fact]
    public void JsonValidator_ValidateProperty_DoesNotValidateOtherProperties()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);
        var model = new TestModel { FirstName = "Alice", Email = "invalid", Age = 25 }; // Email invalid

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.FirstName);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void JsonValidator_ValidateProperty_WithContext_RespectsLayer()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition: definition);
        var model = new TestModel { FirstName = "", Email = "a@b.c", Age = 1 };
        var context = ValidationContext.ForLayer(layer: "entity");

        var result = validator.ValidateProperty(instance: model, propertyExpression: x => x.FirstName, context: context);

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
        Assert.Equal(expected: Severity.NotRecommended, actual: result.Errors[index: 0].Severity);
    }
}
