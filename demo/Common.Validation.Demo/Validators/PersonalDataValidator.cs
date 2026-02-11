using Common.Validation.Core;
using Common.Validation.Demo.Models;
using Common.Validation.Extensions;

namespace Common.Validation.Demo.Validators;

/// <summary>
/// Validates <see cref="PersonalData"/> using fluent validation rules.
/// </summary>
public class PersonalDataValidator : AbstractValidator<PersonalData>
{
    public PersonalDataValidator()
    {
        // Forbidden — these must be present for the record to be processable
        RuleFor(expression: x => x.FirstName)
            .NotEmpty().WithMessage(message: "First name is required.").WithSeverity(severity: Severity.Forbidden)
            .MaxLength(max: 100).WithMessage(message: "First name must not exceed 100 characters.").WithSeverity(severity: Severity.Forbidden);

        RuleFor(expression: x => x.LastName)
            .NotEmpty().WithMessage(message: "Last name is required.").WithSeverity(severity: Severity.Forbidden)
            .MaxLength(max: 100).WithMessage(message: "Last name must not exceed 100 characters.").WithSeverity(severity: Severity.Forbidden);

        RuleFor(expression: x => x.Email)
            .NotEmpty().WithMessage(message: "Email is required.").WithSeverity(severity: Severity.Forbidden)
            .EmailAddress().WithMessage(message: "Invalid email format.").WithSeverity(severity: Severity.Forbidden);

        // AtOwnRisk — phone is strongly recommended but not blocking
        RuleFor(expression: x => x.Phone)
            .NotEmpty().WithMessage(message: "Phone number is recommended.").WithSeverity(severity: Severity.AtOwnRisk)
            .PhoneNumber().WithMessage(message: "Invalid phone number format.").WithSeverity(severity: Severity.AtOwnRisk);

        // Forbidden — citizenship is legally required
        RuleFor(expression: x => x.Citizenship)
            .NotEmpty().WithMessage(message: "Citizenship is required.").WithSeverity(severity: Severity.Forbidden)
            .Length(min: 2, max: 3).WithMessage(message: "Citizenship must be a 2- or 3-letter ISO 3166 country code.").WithSeverity(severity: Severity.Forbidden);

        // NotRecommended — tax residency is nice to have
        RuleFor(expression: x => x.TaxResidency)
            .NotEmpty().WithMessage(message: "Tax residency is recommended for full compliance.").WithSeverity(severity: Severity.NotRecommended)
            .Length(min: 2, max: 3).WithMessage(message: "Tax residency should be a 2- or 3-letter ISO 3166 country code.").WithSeverity(severity: Severity.NotRecommended);
    }
}
