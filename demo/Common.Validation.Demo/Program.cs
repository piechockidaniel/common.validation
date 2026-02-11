using Common.Validation.Core;
using Common.Validation.Demo.Models;
using Common.Validation.Demo.Validators;
using Common.Validation.DependencyInjection;
using Common.Validation.Json;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine(value: "=== Common.Validation Demo ===");
Console.WriteLine();

// ─────────────────────────────────────────────
// Part 1: Fluent validation (original demo)
// ─────────────────────────────────────────────
Console.WriteLine(value: "═══ Part 1: Fluent Validation ═══");
Console.WriteLine();

var validator = new PersonalDataValidator();

Console.WriteLine(value: "--- Scenario 1: Valid personal data ---");
var validPerson = new PersonalData
{
    FirstName = "Jan",
    LastName = "Kowalski",
    Email = "jan.kowalski@example.com",
    Phone = "+48 123 456 789",
    Citizenship = "PL",
    TaxResidency = "PL",
};
PrintResult(result: validator.Validate(instance: validPerson));

Console.WriteLine(value: "--- Scenario 2: All fields empty (shows severity levels) ---");
PrintResult(result: validator.Validate(instance: new PersonalData()));

Console.WriteLine(value: "--- Scenario 3: Missing phone and tax residency (no Forbidden errors) ---");
PrintResult(result: validator.Validate(instance: new PersonalData
{
    FirstName = "Anna",
    LastName = "Nowak",
    Email = "anna.nowak@example.com",
    Phone = "",
    Citizenship = "DE",
    TaxResidency = "",
}));

// ─────────────────────────────────────────────
// Part 2: Multi-layer severity via attributes
// ─────────────────────────────────────────────
Console.WriteLine(value: "═══ Part 2: Multi-Layer Severity ═══");
Console.WriteLine();

var apiValidator = new PersonalDataApiValidator();
var entityValidator = new PersonalDataEntityValidator();

var emptyApiModel = new PersonalDataApiModel();
var emptyEntityModel = new PersonalDataEntity();

Console.WriteLine(value: "--- API layer: empty model (stricter severity) ---");
PrintResult(result: apiValidator.Validate(instance: emptyApiModel));

Console.WriteLine(value: "--- Entity layer: empty model (relaxed severity) ---");
PrintResult(result: entityValidator.Validate(instance: emptyEntityModel));

// ─────────────────────────────────────────────
// Part 3: JSON-based validation
// ─────────────────────────────────────────────
Console.WriteLine(value: "═══ Part 3: JSON-Based Validation ═══");
Console.WriteLine();

var jsonPath = Path.Combine(path1: AppContext.BaseDirectory, path2: "PersonalData.validation.json");
if (File.Exists(path: jsonPath))
{
    var definition = jsonPath.LoadFromFile();
    var jsonValidator = new JsonValidator<PersonalData>(definition: definition);

    Console.WriteLine(value: "--- JSON validator: valid data ---");
    PrintResult(result: jsonValidator.Validate(instance: validPerson));

    Console.WriteLine(value: "--- JSON validator: empty data ---");
    PrintResult(result: jsonValidator.Validate(instance: new PersonalData()));

    Console.WriteLine(value: "--- JSON validator: empty data with 'entity' layer context ---");
    var entityContext = ValidationContext.ForLayer(layer: "entity");
    PrintResult(result: jsonValidator.Validate(instance: new PersonalData(), context: entityContext));
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(value: $"  JSON file not found at: {jsonPath}");
    Console.ResetColor();
}

// ─────────────────────────────────────────────
// Part 4: IoC / Dependency Injection
// ─────────────────────────────────────────────
Console.WriteLine(value: "═══ Part 4: Dependency Injection ═══");
Console.WriteLine();

var services = new ServiceCollection();
services.AddCommonValidation();
services.AddValidatorsFromAssemblyContaining<PersonalDataValidator>();

var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IValidatorFactory>();

var resolvedValidator = factory.GetValidator<PersonalData>();
if (resolvedValidator is not null)
{
    Console.WriteLine(value: "--- Resolved PersonalDataValidator via DI ---");
    PrintResult(result: resolvedValidator.Validate(instance: validPerson));
}

Console.WriteLine();
Console.WriteLine(value: "=== Demo complete ===");

// ─────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────
static void PrintResult(ValidationResult result)
{
    if (result.IsValid)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(value: "  VALID - No errors found.");
        Console.ResetColor();
        Console.WriteLine();
        return;
    }

    Console.ForegroundColor = result.HasForbidden ? ConsoleColor.Red : ConsoleColor.Yellow;
    Console.WriteLine(value: $"  {(result.HasForbidden ? "BLOCKED" : "PASSED WITH WARNINGS")} - {result.Errors.Count} issue(s):");
    Console.ResetColor();

    PrintSeverityGroup(result: result, severity: Severity.Forbidden, color: ConsoleColor.Red);
    PrintSeverityGroup(result: result, severity: Severity.AtOwnRisk, color: ConsoleColor.Yellow);
    PrintSeverityGroup(result: result, severity: Severity.NotRecommended, color: ConsoleColor.DarkGray);

    Console.WriteLine();
}

static void PrintSeverityGroup(ValidationResult result, Severity severity, ConsoleColor color)
{
    var group = result.BySeverity(severity: severity);
    if (group.Count == 0) return;

    Console.ForegroundColor = color;
    Console.WriteLine(value: $"    [{severity}]");
    foreach (var error in group)
    {
        Console.WriteLine(value: $"      {error.PropertyName}: {error.ErrorMessage}");
    }
    Console.ResetColor();
}
