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
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.").WithSeverity(Severity.Forbidden)
            .MaxLength(100).WithMessage("First name must not exceed 100 characters.").WithSeverity(Severity.Forbidden);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.").WithSeverity(Severity.Forbidden)
            .MaxLength(100).WithMessage("Last name must not exceed 100 characters.").WithSeverity(Severity.Forbidden);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.").WithSeverity(Severity.Forbidden)
            .EmailAddress().WithMessage("Invalid email format.").WithSeverity(Severity.Forbidden);

        // AtOwnRisk — phone is strongly recommended but not blocking
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is recommended.").WithSeverity(Severity.AtOwnRisk)
            .PhoneNumber().WithMessage("Invalid phone number format.").WithSeverity(Severity.AtOwnRisk);

        // Forbidden — citizenship is legally required
        RuleFor(x => x.Citizenship)
            .NotEmpty().WithMessage("Citizenship is required.").WithSeverity(Severity.Forbidden)
            .Length(2, 3).WithMessage("Citizenship must be a 2- or 3-letter ISO 3166 country code.").WithSeverity(Severity.Forbidden);

        // NotRecommended — tax residency is nice to have
        RuleFor(x => x.TaxResidency)
            .NotEmpty().WithMessage("Tax residency is recommended for full compliance.").WithSeverity(Severity.NotRecommended)
            .Length(2, 3).WithMessage("Tax residency should be a 2- or 3-letter ISO 3166 country code.").WithSeverity(Severity.NotRecommended);
    }
}
