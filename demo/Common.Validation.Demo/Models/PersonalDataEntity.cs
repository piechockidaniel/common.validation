using Common.Validation.Layers;

namespace Common.Validation.Demo.Models;

/// <summary>
/// Entity layer model for PersonalData.
/// The [ValidationLayer("entity")] attribute causes the validator to
/// automatically apply "entity"-layer severity overrides.
/// </summary>
[ValidationLayer(layer: "entity")]
public class PersonalDataEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Citizenship { get; set; } = string.Empty;
    public string TaxResidency { get; set; } = string.Empty;
}
