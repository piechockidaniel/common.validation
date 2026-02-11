using Common.Validation.Core;
using Common.Validation.Json;

namespace Common.Validation.Tests.Json;

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
        var validator = new JsonValidator<TestModel>(definition);

        var result = validator.Validate(new TestModel
        {
            FirstName = "Alice",
            Email = "alice@example.com",
            Age = 25
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void JsonValidator_InvalidModel_ReturnsFailures()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition);

        var result = validator.Validate(new TestModel
        {
            FirstName = "",
            Email = "not-email",
            Age = 0
        });

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void JsonValidator_RespectsSeverity()
    {
        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition);

        var result = validator.Validate(new TestModel
        {
            FirstName = "",
            Email = "valid@test.com",
            Age = 25
        });

        Assert.Single(result.Errors);
        Assert.Equal(Severity.Forbidden, result.Errors[0].Severity);
    }

    [Fact]
    public void JsonValidator_LayerSeverity_Api()
    {

        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition);

        var context = ValidationContext.ForLayer("api");
        var result = validator.Validate(new TestModel { FirstName = "", Email = "a@b.c", Age = 1 }, context);

        Assert.Single(result.Errors);
        Assert.Equal(Severity.Forbidden, result.Errors[0].Severity);
    }

    [Fact]
    public void JsonValidator_LayerSeverity_Entity()
    {

        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition);

        var context = ValidationContext.ForLayer("entity");
        var result = validator.Validate(new TestModel { FirstName = "", Email = "a@b.c", Age = 1 }, context);

        Assert.Single(result.Errors);
        Assert.Equal(Severity.NotRecommended, result.Errors[0].Severity);
    }

    [Fact]
    public void JsonValidationDefinitionLoader_LoadsDefinition()
    {

        var definition = TestJson.Load();

        Assert.Equal("TestModel", definition.Type);
        Assert.Equal(3, definition.Properties?.Count);
        Assert.True(definition.Properties?.ContainsKey("firstName"));
        Assert.True(definition.Properties?.ContainsKey("email"));
        Assert.True(definition.Properties?.ContainsKey("age"));
    }

    [Fact]
    public void JsonValidator_MaxLength_FailsWhenTooLong()
    {

        var definition = TestJson.Load();
        var validator = new JsonValidator<TestModel>(definition);

        var result = validator.Validate(new TestModel
        {
            FirstName = new string('A', 51),
            Email = "a@b.c",
            Age = 1
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => string.Equals(e.ErrorMessage, "First name too long.", StringComparison.InvariantCultureIgnoreCase));
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
        Assert.Throws<InvalidOperationException>(() => new JsonValidator<TestModel>(definition));
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

        Assert.Throws<InvalidOperationException>(() => new JsonValidator<TestModel>(definition));
    }
}
