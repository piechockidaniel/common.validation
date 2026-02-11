using Common.Validation.Core;
using Common.Validation.Demo.Models;
using Common.Validation.Extensions;

namespace Common.Validation.Demo.Validators;

/// <summary>
/// Entity-layer validator for PersonalData.
/// Same rules but severity resolved from the "entity" layer.
/// </summary>
public class PersonalDataEntityValidator : AbstractValidator<PersonalDataEntity>
{
    public PersonalDataEntityValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .WithSeverity(Severity.Forbidden)
            .WithLayerSeverity("api", Severity.Forbidden)
            .WithLayerSeverity("entity", Severity.NotRecommended);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .WithSeverity(Severity.Forbidden)
            .WithLayerSeverity("api", Severity.Forbidden)
            .WithLayerSeverity("entity", Severity.AtOwnRisk);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is recommended.")
            .WithSeverity(Severity.AtOwnRisk)
            .WithLayerSeverity("api", Severity.AtOwnRisk)
            .WithLayerSeverity("entity", Severity.NotRecommended);
    }
}