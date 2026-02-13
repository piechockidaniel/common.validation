namespace Atlas.Common.Validation.Demo.Models;

/// <summary>
/// Represents a personal data record subject to validation.
/// </summary>
public class PersonalData
{
    /// <summary>First name of the person.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name / surname of the person.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number (international format accepted).</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>ISO 3166 country code for citizenship (2 or 3 letters).</summary>
    public string Citizenship { get; set; } = string.Empty;

    /// <summary>ISO 3166 country code for tax residency (2 or 3 letters).</summary>
    public string TaxResidency { get; set; } = string.Empty;
}
