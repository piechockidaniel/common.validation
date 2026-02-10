using Common.Validation.Core;
using Common.Validation.Demo.Models;
using Common.Validation.Demo.Validators;
using Common.Validation.DependencyInjection;
using Common.Validation.Json;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== Common.Validation Demo ===");
Console.WriteLine();

// ─────────────────────────────────────────────
// Part 1: Fluent validation (original demo)
// ─────────────────────────────────────────────
Console.WriteLine("═══ Part 1: Fluent Validation ═══");
Console.WriteLine();

var validator = new PersonalDataValidator();

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
PrintResult(validator.Validate(validPerson));

Console.WriteLine("--- Scenario 2: All fields empty (shows severity levels) ---");
PrintResult(validator.Validate(new PersonalData()));

Console.WriteLine("--- Scenario 3: Missing phone and tax residency (no Forbidden errors) ---");
PrintResult(validator.Validate(new PersonalData
{
    FirstName = "Anna",
    LastName = "Nowak",
    Email = "anna.nowak@example.com",
    Phone = "",
    Citizenship = "DE",
    TaxResidency = ""
}));

// ─────────────────────────────────────────────
// Part 2: Multi-layer severity via attributes
// ─────────────────────────────────────────────
Console.WriteLine("═══ Part 2: Multi-Layer Severity ═══");
Console.WriteLine();

var apiValidator = new PersonalDataApiValidator();
var entityValidator = new PersonalDataEntityValidator();

var emptyApiModel = new PersonalDataApiModel();
var emptyEntityModel = new PersonalDataEntity();

Console.WriteLine("--- API layer: empty model (stricter severity) ---");
PrintResult(apiValidator.Validate(emptyApiModel));

Console.WriteLine("--- Entity layer: empty model (relaxed severity) ---");
PrintResult(entityValidator.Validate(emptyEntityModel));

// ─────────────────────────────────────────────
// Part 3: JSON-based validation
// ─────────────────────────────────────────────
Console.WriteLine("═══ Part 3: JSON-Based Validation ═══");
Console.WriteLine();

var jsonPath = Path.Combine(AppContext.BaseDirectory, "PersonalData.validation.json");
if (File.Exists(jsonPath))
{
    var loader = new JsonValidationDefinitionLoader();
    var definition = loader.LoadFromFile(jsonPath);
    var jsonValidator = new JsonValidator<PersonalData>(definition);

    Console.WriteLine("--- JSON validator: valid data ---");
    PrintResult(jsonValidator.Validate(validPerson));

    Console.WriteLine("--- JSON validator: empty data ---");
    PrintResult(jsonValidator.Validate(new PersonalData()));

    Console.WriteLine("--- JSON validator: empty data with 'entity' layer context ---");
    var entityContext = ValidationContext.ForLayer("entity");
    PrintResult(jsonValidator.Validate(new PersonalData(), entityContext));
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  JSON file not found at: {jsonPath}");
    Console.ResetColor();
}

// ─────────────────────────────────────────────
// Part 4: IoC / Dependency Injection
// ─────────────────────────────────────────────
Console.WriteLine("═══ Part 4: Dependency Injection ═══");
Console.WriteLine();

var services = new ServiceCollection();
services.AddCommonValidation();
services.AddValidatorsFromAssemblyContaining<PersonalDataValidator>();

var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IValidatorFactory>();

var resolvedValidator = factory.GetValidator<PersonalData>();
if (resolvedValidator is not null)
{
    Console.WriteLine("--- Resolved PersonalDataValidator via DI ---");
    PrintResult(resolvedValidator.Validate(validPerson));
}

Console.WriteLine();
Console.WriteLine("=== Demo complete ===");

// ─────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────
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

    Console.ForegroundColor = result.HasForbidden ? ConsoleColor.Red : ConsoleColor.Yellow;
    Console.WriteLine($"  {(result.HasForbidden ? "BLOCKED" : "PASSED WITH WARNINGS")} - {result.Errors.Count} issue(s):");
    Console.ResetColor();

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
