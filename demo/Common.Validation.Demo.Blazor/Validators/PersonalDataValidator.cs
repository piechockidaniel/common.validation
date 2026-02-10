using Common.Validation.Core;
using Common.Validation.Demo.Blazor.Models;
using Common.Validation.Extensions;

namespace Common.Validation.Demo.Blazor.Validators;

public class PersonalDataValidator : AbstractValidator<PersonalData>
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
            .Length(2, 3).WithMessage("Must be a 2- or 3-letter ISO 3166 code.").WithSeverity(Severity.Forbidden);

        RuleFor(x => x.TaxResidency)
            .NotEmpty().WithMessage("Tax residency is recommended.").WithSeverity(Severity.NotRecommended)
            .Length(2, 3).WithMessage("Should be a 2- or 3-letter ISO 3166 code.").WithSeverity(Severity.NotRecommended);
    }
}
