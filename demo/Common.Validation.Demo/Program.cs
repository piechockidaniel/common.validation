using Common.Validation.Core;
using Common.Validation.Demo.Models;
using Common.Validation.Demo.Validators;

Console.WriteLine("=== Common.Validation Demo ===");
Console.WriteLine();

var validator = new PersonalDataValidator();

// --- Scenario 1: Valid data ---
Console.WriteLine("--- Scenario 1: Valid personal data ---");
var validPerson = new PersonalData
{
    FirstName = "Jan",
    LastName = "Kowalski",
    Email = "jan.kowalski@example.com",
    Phone = "+48 123 456 789",
    Citizenship = "PL",
    TaxResidency = "PL"
};

var result1 = validator.Validate(validPerson);
PrintResult(result1);

// --- Scenario 2: Completely empty data (mixed severities) ---
Console.WriteLine("--- Scenario 2: All fields empty (shows severity levels) ---");
var emptyPerson = new PersonalData();

var result2 = validator.Validate(emptyPerson);
PrintResult(result2);

// --- Scenario 3: Missing only optional / advisory fields ---
Console.WriteLine("--- Scenario 3: Missing phone and tax residency (no Forbidden errors) ---");
var partialPerson = new PersonalData
{
    FirstName = "Anna",
    LastName = "Nowak",
    Email = "anna.nowak@example.com",
    Phone = "",          // AtOwnRisk
    Citizenship = "DE",
    TaxResidency = ""   // NotRecommended
};

var result3 = validator.Validate(partialPerson);
PrintResult(result3);

// --- Scenario 4: Invalid email and bad country codes ---
Console.WriteLine("--- Scenario 4: Invalid email and country codes ---");
var badDataPerson = new PersonalData
{
    FirstName = "Maria",
    LastName = "Garcia",
    Email = "not-an-email",
    Phone = "+34 600 123 456",
    Citizenship = "SPAIN",
    TaxResidency = "PORTUGAL"
};

var result4 = validator.Validate(badDataPerson);
PrintResult(result4);

Console.WriteLine("=== Demo complete ===");

static void PrintResult(ValidationResult result)
{
    if (result.IsValid)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  VALID - No errors found.");
        Console.ResetColor();
        Console.WriteLine();
        return;
    }

    // Summary line
    Console.ForegroundColor = result.HasForbidden ? ConsoleColor.Red : ConsoleColor.Yellow;
    Console.WriteLine($"  {(result.HasForbidden ? "BLOCKED" : "PASSED WITH WARNINGS")} - {result.Errors.Count} issue(s):");
    Console.ResetColor();

    // Print grouped by severity
    PrintSeverityGroup(result, Severity.Forbidden, ConsoleColor.Red);
    PrintSeverityGroup(result, Severity.AtOwnRisk, ConsoleColor.Yellow);
    PrintSeverityGroup(result, Severity.NotRecommended, ConsoleColor.DarkGray);

    Console.WriteLine();
}

static void PrintSeverityGroup(ValidationResult result, Severity severity, ConsoleColor color)
{
    var group = result.BySeverity(severity);
    if (group.Count == 0) return;

    Console.ForegroundColor = color;
    Console.WriteLine($"    [{severity}]");
    foreach (var error in group)
    {
        Console.WriteLine($"      {error.PropertyName}: {error.ErrorMessage}");
    }
    Console.ResetColor();
}
