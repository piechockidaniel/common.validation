using Common.Validation.Core;
using Common.Validation.Extensions;

namespace Common.Validation.Tests.Integration;

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
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.").WithSeverity(Severity.Forbidden)
                .MaxLength(100).WithMessage("First name must not exceed 100 characters.").WithSeverity(Severity.Forbidden);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.").WithSeverity(Severity.Forbidden)
                .MaxLength(100).WithMessage("Last name must not exceed 100 characters.").WithSeverity(Severity.Forbidden);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.").WithSeverity(Severity.Forbidden)
                .EmailAddress().WithMessage("Invalid email format.").WithSeverity(Severity.Forbidden);

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is recommended.").WithSeverity(Severity.AtOwnRisk)
                .PhoneNumber().WithMessage("Invalid phone number format.").WithSeverity(Severity.AtOwnRisk);

            RuleFor(x => x.Citizenship)
                .NotEmpty().WithMessage("Citizenship is required.").WithSeverity(Severity.Forbidden)
                .Length(2, 3).WithMessage("Citizenship must be a 2- or 3-letter ISO 3166 country code.").WithSeverity(Severity.Forbidden);

            RuleFor(x => x.TaxResidency)
                .NotEmpty().WithMessage("Tax residency is recommended for full compliance.").WithSeverity(Severity.NotRecommended)
                .Length(2, 3).WithMessage("Tax residency should be a 2- or 3-letter ISO 3166 country code.").WithSeverity(Severity.NotRecommended);
        }
    }

    [Fact]
    public void Scenario1_ValidData_NoErrors()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(new PersonalData
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "+48 123 456 789",
            Citizenship = "PL",
            TaxResidency = "PL"
        });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Scenario2_AllEmpty_MixedSeverities()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(new PersonalData());

        Assert.False(result.IsValid);
        Assert.True(result.HasForbidden);
        Assert.True(result.HasAtOwnRisk);
        Assert.True(result.HasNotRecommended);

        // Forbidden: FirstName(NotEmpty), LastName(NotEmpty), Email(NotEmpty+EmailAddress), 
        // Citizenship(NotEmpty+Length) = 6 Forbidden failures
        Assert.Equal(6, result.BySeverity(Severity.Forbidden).Count);
        // AtOwnRisk: Phone(NotEmpty+PhoneNumber) = 2
        Assert.Equal(2, result.BySeverity(Severity.AtOwnRisk).Count);
        // NotRecommended: TaxResidency(NotEmpty+Length) = 2
        Assert.Equal(2, result.BySeverity(Severity.NotRecommended).Count);
    }

    [Fact]
    public void Scenario3_MissingOptionalOnly_NoForbiddenErrors()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(new PersonalData
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna.nowak@example.com",
            Phone = "",
            Citizenship = "DE",
            TaxResidency = ""
        });

        Assert.False(result.IsValid);
        Assert.False(result.HasForbidden);
        Assert.True(result.HasAtOwnRisk);
        Assert.True(result.HasNotRecommended);
    }

    [Fact]
    public void Scenario4_InvalidFormats_ReturnsFormatErrors()
    {
        var validator = new PersonalDataValidator();
        var result = validator.Validate(new PersonalData
        {
            FirstName = "Maria",
            LastName = "Garcia",
            Email = "not-an-email",
            Phone = "+34 600 123 456",
            Citizenship = "SPAIN",
            TaxResidency = "PORTUGAL"
        });

        Assert.False(result.IsValid);
        Assert.True(result.HasForbidden);

        // Email format and Citizenship/TaxResidency length errors
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage.Contains("email format"));
        Assert.Contains(result.Errors, e => e.PropertyName == "Citizenship" && e.ErrorMessage.Contains("country code"));
    }
}
