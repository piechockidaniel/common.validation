using Common.Validation.Layers;

namespace Common.Validation.Demo.Models;

/// <summary>
/// API layer model for PersonalData.
/// The [ValidationLayer("api")] attribute causes the validator to
/// automatically apply "api"-layer severity overrides.
/// </summary>
[ValidationLayer(layer: "api")]
public class PersonalDataApiModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Citizenship { get; set; } = string.Empty;
    public string TaxResidency { get; set; } = string.Empty;
}
