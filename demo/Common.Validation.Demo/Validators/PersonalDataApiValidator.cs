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