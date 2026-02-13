using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Demo.Blazor.Models;
using Atlas.Common.Validation.Extensions;

namespace Atlas.Common.Validation.Demo.Blazor.Validators;

public class PersonalDataValidator : AbstractValidator<PersonalData>
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
            .Length(min: 2, max: 3).WithMessage(message: "Must be a 2- or 3-letter ISO 3166 code.").WithSeverity(severity: Severity.Forbidden);

        RuleFor(expression: x => x.TaxResidency)
            .NotEmpty().WithMessage(message: "Tax residency is recommended.").WithSeverity(severity: Severity.NotRecommended)
            .Length(min: 2, max: 3).WithMessage(message: "Should be a 2- or 3-letter ISO 3166 code.").WithSeverity(severity: Severity.NotRecommended);
    }
}
