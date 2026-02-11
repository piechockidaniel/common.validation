using Common.Validation.Core;
using Common.Validation.Demo.Models;
using Common.Validation.Extensions;

namespace Common.Validation.Demo.Validators;

/// <summary>
/// Validates PersonalData models with layer-aware severity.
/// The same rules are defined once, but severity differs per layer.
/// </summary>
public class PersonalDataApiValidator : AbstractValidator<PersonalDataApiModel>
{
    public PersonalDataApiValidator()
    {
        ConfigureRules();
    }

    private void ConfigureRules()
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