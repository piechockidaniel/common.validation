using Common.Validation.Core;

namespace Common.Validation.Tests.Core;

public class ValidationResultTests
{
    [Fact]
    public void EmptyResult_IsValid()
    {
        var result = new ValidationResult();
        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void ResultWithErrors_IsNotValid()
    {
        var result = new ValidationResult(errors:
        [
            new ValidationFailure(propertyName: "Name", errorMessage: "Required")
        ]);

        Assert.False(condition: result.IsValid);
        Assert.Single(collection: result.Errors);
    }

    [Fact]
    public void HasForbidden_ReturnsTrue_WhenForbiddenErrorsExist()
    {
        var result = new ValidationResult(errors:
        [
            new ValidationFailure(propertyName: "Name", errorMessage: "Required") { Severity = Severity.Forbidden }
        ]);

        Assert.True(condition: result.HasForbidden);
        Assert.False(condition: result.HasAtOwnRisk);
        Assert.False(condition: result.HasNotRecommended);
    }

    [Fact]
    public void HasAtOwnRisk_ReturnsTrue_WhenAtOwnRiskErrorsExist()
    {
        var result = new ValidationResult(errors:
        [
            new ValidationFailure(propertyName: "Phone", errorMessage: "Recommended") { Severity = Severity.AtOwnRisk }
        ]);

        Assert.False(condition: result.HasForbidden);
        Assert.True(condition: result.HasAtOwnRisk);
    }

    [Fact]
    public void HasNotRecommended_ReturnsTrue_WhenNotRecommendedErrorsExist()
    {
        var result = new ValidationResult(errors:
        [
            new ValidationFailure(propertyName: "Tax", errorMessage: "Nice to have") { Severity = Severity.NotRecommended }
        ]);

        Assert.True(condition: result.HasNotRecommended);
    }

    [Fact]
    public void BySeverity_FiltersCorrectly()
    {
        var result = new ValidationResult(errors:
        [
            new ValidationFailure(propertyName: "A", errorMessage: "err") { Severity = Severity.Forbidden },
            new ValidationFailure(propertyName: "B", errorMessage: "warn") { Severity = Severity.AtOwnRisk },
            new ValidationFailure(propertyName: "C", errorMessage: "info") { Severity = Severity.NotRecommended },
            new ValidationFailure(propertyName: "D", errorMessage: "err2") { Severity = Severity.Forbidden }
        ]);

        Assert.Equal(expected: 2, actual: result.BySeverity(severity: Severity.Forbidden).Count);
        Assert.Single(collection: result.BySeverity(severity: Severity.AtOwnRisk));
        Assert.Single(collection: result.BySeverity(severity: Severity.NotRecommended));
    }

    [Fact]
    public void Merge_CombinesTwoResults()
    {
        var r1 = new ValidationResult(errors: [new ValidationFailure(propertyName: "A", errorMessage: "err1")]);
        var r2 = new ValidationResult(errors: [new ValidationFailure(propertyName: "B", errorMessage: "err2")]);

        var merged = r1.Merge(other: r2);

        Assert.Equal(expected: 2, actual: merged.Errors.Count);
        Assert.Contains(collection: merged.Errors, filter: e => string.Equals(a: e.PropertyName, b: "A", comparisonType: StringComparison.InvariantCultureIgnoreCase));
        Assert.Contains(collection: merged.Errors, filter: e => string.Equals(a: e.PropertyName, b: "B", comparisonType: StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public void Combine_CombinesMultipleResults()
    {
        var r1 = new ValidationResult(errors: [new ValidationFailure(propertyName: "A", errorMessage: "err1")]);
        var r2 = new ValidationResult(errors: [new ValidationFailure(propertyName: "B", errorMessage: "err2")]);
        var r3 = new ValidationResult(errors: [new ValidationFailure(propertyName: "C", errorMessage: "err3")]);

        var combined = ValidationResult.Combine(results: [r1, r2, r3]);

        Assert.Equal(expected: 3, actual: combined.Errors.Count);
    }

    [Fact]
    public void Combine_EmptyResults_ReturnsValid()
    {
        var combined = ValidationResult.Combine(results: [new ValidationResult(), new ValidationResult()]);
        Assert.True(condition: combined.IsValid);
    }

    [Fact]
    public void ToString_Valid_ReturnsSuccessMessage()
    {
        var result = new ValidationResult();
        Assert.Equal(expected: "Validation succeeded.", actual: result.ToString());
    }

    [Fact]
    public void ToString_Invalid_ContainsErrorCount()
    {
        var result = new ValidationResult(errors:
        [
            new ValidationFailure(propertyName: "A", errorMessage: "err1"),
            new ValidationFailure(propertyName: "B", errorMessage: "err2")
        ]);

        Assert.Contains(expectedSubstring: "2 error(s)", actualString: result.ToString(), comparisonType: StringComparison.CurrentCultureIgnoreCase);
    }
}
