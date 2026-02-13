using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Extensions;

namespace Atlas.Common.Validation.Tests.Integration;

/// <summary>
/// Integration tests matching the demo app scenarios.
/// </summary>
public class PersonalDataIntegrationTests
{
    private class PersonalData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Citizenship { get; set; } = string.Empty;
        public string TaxResidency { get; set; } = string.Empty;
    }

    private class PersonalDataValidator : AbstractValidator<PersonalData>
    {
        public PersonalDataValidator()
        {
            RuleFor(expression: x => x.FirstName)
                .NotEmpty().WithMessage(message: "First name is required.").WithSeverity(severity: Severity.Forbidden)
                .MaxLength(max: 100).WithMessage(message: "First name must not exceed 100 characters.").WithSeverity(severity: Severity.Forbidden);

            RuleFor(expression: x => x.LastName)
                .NotEmpty().WithMessage(message: "Last name is required.").WithSeverity(severity: Severity.Forbidden)
                .MaxLength(max: 100).WithMessage(message: "Last name must not exceed 100 characters.").WithSeverity(severity: Severity.Forbidden);

            RuleFor(expression: x => x.Email)
                .NotEmpty().WithMessage(message: "Email is required.").WithSeverity(severity: Severity.Forbidden)
                .EmailAddress().WithMessage(message: "Invalid email format.").WithSeverity(severity: Severity.Forbidden);

            RuleFor(expression: x => x.Phone)
                .NotEmpty().WithMessage(message: "Phone number is recommended.").WithSeverity(severity: Severity.AtOwnRisk)
                .PhoneNumber().WithMessage(message: "Invalid phone number format.").WithSeverity(severity: Severity.AtOwnRisk);

            RuleFor(expression: x => x.Citizenship)
                .NotEmpty().WithMessage(message: "Citizenship is required.").WithSeverity(severity: Severity.Forbidden)
                .Length(min: 2, max: 3).WithMessage(message: "Citizenship must be a 2- or 3-letter ISO 3166 country code.").WithSeverity(severity: Severity.Forbidden);

            RuleFor(expression: x => x.TaxResidency)
                .NotEmpty().WithMessage(message: "Tax residency is recommended for full compliance.").WithSeverity(severity: Severity.NotRecommended)
                .Length(min: 2, max: 3).WithMessage(message: "Tax residency should be a 2- or 3-letter ISO 3166 country code.").WithSeverity(severity: Severity.NotRecommended);
        }
    }

    [Fact]
    public void Scenario1_ValidData_NoErrors()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(instance: new PersonalData
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "+48 123 456 789",
            Citizenship = "PL",
            TaxResidency = "PL"
        });

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public void Scenario2_AllEmpty_MixedSeverities()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(instance: new PersonalData());

        Assert.False(condition: result.IsValid);
        Assert.True(condition: result.HasForbidden);
        Assert.True(condition: result.HasAtOwnRisk);
        Assert.True(condition: result.HasNotRecommended);

        // Forbidden: FirstName(NotEmpty), LastName(NotEmpty), Email(NotEmpty+EmailAddress), 
        // Citizenship(NotEmpty+Length) = 6 Forbidden failures
        Assert.Equal(expected: 6, actual: result.BySeverity(severity: Severity.Forbidden).Count);
        // AtOwnRisk: Phone(NotEmpty+PhoneNumber) = 2
        Assert.Equal(expected: 2, actual: result.BySeverity(severity: Severity.AtOwnRisk).Count);
        // NotRecommended: TaxResidency(NotEmpty+Length) = 2
        Assert.Equal(expected: 2, actual: result.BySeverity(severity: Severity.NotRecommended).Count);
    }

    [Fact]
    public void Scenario3_MissingOptionalOnly_NoForbiddenErrors()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(instance: new PersonalData
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna.nowak@example.com",
            Phone = "",
            Citizenship = "DE",
            TaxResidency = ""
        });

        Assert.False(condition: result.IsValid);
        Assert.False(condition: result.HasForbidden);
        Assert.True(condition: result.HasAtOwnRisk);
        Assert.True(condition: result.HasNotRecommended);
    }

    [Fact]
    public void Scenario4_InvalidFormats_ReturnsFormatErrors()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(instance: new PersonalData
        {
            FirstName = "Maria",
            LastName = "Garcia",
            Email = "not-an-email",
            Phone = "+34 600 123 456",
            Citizenship = "SPAIN",
            TaxResidency = "PORTUGAL"
        });

        Assert.False(condition: result.IsValid);
        Assert.True(condition: result.HasForbidden);

        // Email format and Citizenship/TaxResidency length errors
        Assert.Contains(collection: result.Errors, filter: e => string.Equals(a: e.PropertyName, b: "Email", comparisonType: StringComparison.InvariantCultureIgnoreCase) && e.ErrorMessage.Contains(value: "email format"));
        Assert.Contains(collection: result.Errors, filter: e => string.Equals(a: e.PropertyName, b: "Citizenship", comparisonType: StringComparison.InvariantCultureIgnoreCase) && e.ErrorMessage.Contains(value: "country code"));
    }
}
