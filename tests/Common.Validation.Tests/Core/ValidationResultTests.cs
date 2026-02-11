using Common.Validation.Core;

namespace Common.Validation.Tests.Core;

public class ValidationResultTests
{
    [Fact]
    public void EmptyResult_IsValid()
    {
        var result = new ValidationResult();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ResultWithErrors_IsNotValid()
    {
        var result = new ValidationResult([
            new ValidationFailure("Name", "Required")
        ]);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void HasForbidden_ReturnsTrue_WhenForbiddenErrorsExist()
    {
        var result = new ValidationResult([
            new ValidationFailure("Name", "Required") { Severity = Severity.Forbidden }
        ]);

        Assert.True(result.HasForbidden);
        Assert.False(result.HasAtOwnRisk);
        Assert.False(result.HasNotRecommended);
    }

    [Fact]
    public void HasAtOwnRisk_ReturnsTrue_WhenAtOwnRiskErrorsExist()
    {
        var result = new ValidationResult([
            new ValidationFailure("Phone", "Recommended") { Severity = Severity.AtOwnRisk }
        ]);

        Assert.False(result.HasForbidden);
        Assert.True(result.HasAtOwnRisk);
    }

    [Fact]
    public void HasNotRecommended_ReturnsTrue_WhenNotRecommendedErrorsExist()
    {
        var result = new ValidationResult([
            new ValidationFailure("Tax", "Nice to have") { Severity = Severity.NotRecommended }
        ]);

        Assert.True(result.HasNotRecommended);
    }

    [Fact]
    public void BySeverity_FiltersCorrectly()
    {
        var result = new ValidationResult([
            new ValidationFailure("A", "err") { Severity = Severity.Forbidden },
            new ValidationFailure("B", "warn") { Severity = Severity.AtOwnRisk },
            new ValidationFailure("C", "info") { Severity = Severity.NotRecommended },
            new ValidationFailure("D", "err2") { Severity = Severity.Forbidden }
        ]);

        Assert.Equal(2, result.BySeverity(Severity.Forbidden).Count);
        Assert.Single(result.BySeverity(Severity.AtOwnRisk));
        Assert.Single(result.BySeverity(Severity.NotRecommended));
    }

    [Fact]
    public void Merge_CombinesTwoResults()
    {
        var r1 = new ValidationResult([new ValidationFailure("A", "err1")]);
        var r2 = new ValidationResult([new ValidationFailure("B", "err2")]);

        var merged = r1.Merge(r2);

        Assert.Equal(2, merged.Errors.Count);
        Assert.Contains(merged.Errors, e => string.Equals(e.PropertyName, "A", StringComparison.InvariantCultureIgnoreCase));
        Assert.Contains(merged.Errors, e => string.Equals(e.PropertyName, "B", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public void Combine_CombinesMultipleResults()
    {
        var r1 = new ValidationResult([new ValidationFailure("A", "err1")]);
        var r2 = new ValidationResult([new ValidationFailure("B", "err2")]);
        var r3 = new ValidationResult([new ValidationFailure("C", "err3")]);

        var combined = ValidationResult.Combine(r1, r2, r3);

        Assert.Equal(3, combined.Errors.Count);
    }

    [Fact]
    public void Combine_EmptyResults_ReturnsValid()
    {
        var combined = ValidationResult.Combine(new ValidationResult(), new ValidationResult());
        Assert.True(combined.IsValid);
    }

    [Fact]
    public void ToString_Valid_ReturnsSuccessMessage()
    {
        var result = new ValidationResult();
        Assert.Equal("Validation succeeded.", result.ToString());
    }

    [Fact]
    public void ToString_Invalid_ContainsErrorCount()
    {
        var result = new ValidationResult([
            new ValidationFailure("A", "err1"),
            new ValidationFailure("B", "err2")
        ]);

        Assert.Contains("2 error(s)", result.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}
