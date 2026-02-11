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
        RuleFor(expression: x => x.FirstName)
            .NotEmpty().WithMessage(message: "First name is required.")
            .WithSeverity(severity: Severity.Forbidden)
            .WithLayerSeverity(layer: "api", severity: Severity.Forbidden)
            .WithLayerSeverity(layer: "entity", severity: Severity.NotRecommended);

        RuleFor(expression: x => x.Email)
            .NotEmpty().WithMessage(message: "Email is required.")
            .WithSeverity(severity: Severity.Forbidden)
            .WithLayerSeverity(layer: "api", severity: Severity.Forbidden)
            .WithLayerSeverity(layer: "entity", severity: Severity.AtOwnRisk);

        RuleFor(expression: x => x.Phone)
            .NotEmpty().WithMessage(message: "Phone number is recommended.")
            .WithSeverity(severity: Severity.AtOwnRisk)
            .WithLayerSeverity(layer: "api", severity: Severity.AtOwnRisk)
            .WithLayerSeverity(layer: "entity", severity: Severity.NotRecommended);
    }
}