# Common.Validation

An interoperable, multi-layer validation framework for .NET 10 and TypeScript. Define validation rules once -- in C# or JSON -- and enforce them across API models, DTOs, database entities, and frontend forms with per-layer severity control.

## Table of Contents

- [Key Concepts](#key-concepts)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Your First Validator](#your-first-validator)
  - [Interpreting Results](#interpreting-results)
- [Fluent API Reference](#fluent-api-reference)
  - [Built-in Rules](#built-in-rules)
  - [Custom Rules](#custom-rules)
  - [Conditions](#conditions)
  - [Cascade Mode](#cascade-mode)
- [Multi-Layer Severity](#multi-layer-severity)
  - [The Problem](#the-problem)
  - [Layer Attributes](#layer-attributes)
  - [Layer-Aware Rules](#layer-aware-rules)
  - [Explicit Context](#explicit-context)
- [JSON-Based Validation](#json-based-validation)
  - [Schema Overview](#schema-overview)
  - [Loading Definitions](#loading-definitions)
  - [Custom Validator Types](#custom-validator-types)
- [Dependency Injection](#dependency-injection)
  - [Basic Setup](#basic-setup)
  - [Assembly Scanning](#assembly-scanning)
  - [Validator Factory](#validator-factory)
- [Property-Level Validation](#property-level-validation)
- [Standalone Value Validation](#standalone-value-validation)
  - [Class-Based Value Validators](#class-based-value-validators)
  - [Inline Factory](#inline-factory)
  - [Standalone Rules & Modifiers](#standalone-rules--modifiers)
  - [Layer Support in Standalone Mode](#layer-support-in-standalone-mode)
  - [Value-Based Conditions](#value-based-conditions)
  - [When to Use Standalone vs Object Validation](#when-to-use-standalone-vs-object-validation)
- [Blazor Integration](#blazor-integration)
  - [Components](#components)
  - [EditContext Integration](#editcontext-integration)
- [TypeScript Client](#typescript-client)
- [Architecture](#architecture)
- [Use Cases](#use-cases)
- [Roadmap](#roadmap)
- [License](#license)

---

## Key Concepts

Common.Validation is built around three ideas that distinguish it from other validation libraries:

**Severity is not binary.** Validation failures are not just "valid" or "invalid". Each failure carries a `Severity`:

| Severity | Meaning | Typical Action |
|---|---|---|
| `Forbidden` | The value is invalid. The operation must not proceed. | Block the request. |
| `AtOwnRisk` | The value is risky. The caller accepts responsibility. | Warn the user, log it, proceed. |
| `NotRecommended` | The value is technically valid but not ideal. | Show an informational hint. |

**Layers change severity.** The same validation rule can produce different severities depending on where it runs. A phone number might be `Forbidden` to omit at the API layer but merely `NotRecommended` in a database entity.

**One definition, many runtimes.** Rules defined in a shared JSON schema are consumed by both the C# backend and TypeScript frontend, ensuring client-server parity without duplicating logic.

---

## Getting Started

### Installation

Add the NuGet package to your project:

```bash
dotnet add package Common.Validation
```

For Blazor components:

```bash
dotnet add package Common.Validation.Blazor
```

For TypeScript/JavaScript:

```bash
npm install common-validation
```

### Your First Validator

Create a model and a validator:

```csharp
using Common.Validation.Core;
using Common.Validation.Extensions;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string? PromoCode { get; set; }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaxLength(200).WithMessage("Name is too long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Not a valid email address.");

        RuleFor(x => x.Total)
            .GreaterThan(0m).WithMessage("Order total must be positive.");

        RuleFor(x => x.PromoCode)
            .MaxLength(20).WithMessage("Promo code is too long.")
            .WithSeverity(Severity.NotRecommended);
    }
}
```

Use it:

```csharp
var validator = new CreateOrderValidator();
var result = validator.Validate(new CreateOrderRequest
{
    CustomerName = "",
    Email = "bad",
    Total = -5m
});

if (result.HasForbidden)
{
    // Block: mandatory fields are missing or invalid
    foreach (var error in result.BySeverity(Severity.Forbidden))
        Console.WriteLine($"  ERROR: {error.PropertyName} - {error.ErrorMessage}");
}
```

### Interpreting Results

`ValidationResult` provides several ways to inspect failures:

```csharp
result.IsValid              // true if zero failures
result.HasForbidden         // any blocking errors?
result.HasAtOwnRisk         // any risk warnings?
result.HasNotRecommended    // any soft hints?
result.Errors               // all failures as IReadOnlyList<ValidationFailure>
result.BySeverity(severity) // filter by severity level
```

You can combine results from multiple validators:

```csharp
var combined = ValidationResult.Combine(
    addressValidator.Validate(order.Address),
    paymentValidator.Validate(order.Payment)
);
```

---

## Fluent API Reference

### Built-in Rules

**Common rules** (any property type):

| Method | Description |
|---|---|
| `.NotNull()` | Value must not be null |
| `.Null()` | Value must be null |
| `.NotEmpty()` | String not null/whitespace, collection not empty |
| `.Empty()` | Inverse of NotEmpty |
| `.Equal(value)` | Must equal the given value |
| `.NotEqual(value)` | Must not equal the given value |
| `.Must(predicate, msg)` | Custom predicate |

**String rules:**

| Method | Description |
|---|---|
| `.MinLength(n)` | At least n characters |
| `.MaxLength(n)` | At most n characters |
| `.Length(min, max)` | Between min and max characters |
| `.Matches(pattern)` | Matches a regex pattern |
| `.EmailAddress()` | Valid email format |
| `.PhoneNumber()` | Valid phone format |

**Comparison rules** (for `IComparable<T>`):

| Method | Description |
|---|---|
| `.GreaterThan(n)` | Strictly greater than n |
| `.GreaterThanOrEqual(n)` | Greater than or equal to n |
| `.LessThan(n)` | Strictly less than n |
| `.LessThanOrEqual(n)` | Less than or equal to n |
| `.InclusiveBetween(a, b)` | Between a and b (inclusive) |

**Modifiers** (chain after any rule):

| Method | Description |
|---|---|
| `.WithMessage("...")` | Custom error message |
| `.WithErrorCode("...")` | Programmatic error code |
| `.WithSeverity(Severity.X)` | Set default severity |
| `.WithLayerSeverity("api", Severity.X)` | Layer-specific severity |
| `.Cascade(CascadeMode.StopOnFirstFailure)` | Stop on first failure for this property |

### Custom Rules

Use `.Must()` for inline predicates:

```csharp
RuleFor(x => x.StartDate)
    .Must(date => date >= DateTime.Today, "Start date must be in the future.");
```

Access the parent object for cross-property validation:

```csharp
RuleFor(x => x.EndDate)
    .Must((order, endDate) => endDate > order.StartDate,
          "End date must be after start date.");
```

### Conditions

Apply rules conditionally:

```csharp
RuleFor(x => x.CompanyName)
    .When(x => x.CustomerType == CustomerType.Business)
    .NotEmpty().WithMessage("Company name is required for business customers.");

RuleFor(x => x.PersonalId)
    .Unless(x => x.CustomerType == CustomerType.Business)
    .NotEmpty().WithMessage("Personal ID is required for individual customers.");
```

### Cascade Mode

Control whether validation continues after a failure:

```csharp
// Validator level: stop after the first rule with failures
public class StrictValidator : AbstractValidator<Order>
{
    public StrictValidator()
    {
        CascadeMode = CascadeMode.StopOnFirstFailure;
        // ...
    }
}

// Property level: stop checking this property after first failure
RuleFor(x => x.Email)
    .Cascade(CascadeMode.StopOnFirstFailure)
    .NotEmpty().WithMessage("Required.")
    .EmailAddress().WithMessage("Invalid format.");
    // If NotEmpty fails, EmailAddress is skipped
```

---

## Multi-Layer Severity

### The Problem

In a typical enterprise application, the same data passes through multiple layers:

```
Browser Form  -->  API Controller  -->  Service Layer  -->  Database Entity
```

Each layer has different constraints. An email address might be:
- **Forbidden** to omit at the API (you can't create a user without one)
- **AtOwnRisk** at the DTO level (partial updates may skip it)
- **NotRecommended** at the entity level (legacy records might lack one)

Common.Validation solves this with per-layer severity overrides.

### Layer Attributes

Mark your models with the layer they belong to:

```csharp
using Common.Validation.Layers;

[ValidationLayer("api")]
public class UserApiModel
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

[ValidationLayer("dto")]
public class UserDto
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

[ValidationLayer("entity")]
public class UserEntity
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
```

The attribute is inherited, so base class attributes flow to subclasses:

```csharp
[ValidationLayer("api")]
public abstract class ApiModelBase { }

public class UserApiModel : ApiModelBase { }
// UserApiModel automatically resolves to the "api" layer
```

### Layer-Aware Rules

Define rules once with per-layer severity overrides:

```csharp
public class UserValidator : AbstractValidator<UserApiModel>
{
    public UserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .WithSeverity(Severity.Forbidden)               // default
            .WithLayerSeverity("api", Severity.Forbidden)    // mandatory at API
            .WithLayerSeverity("dto", Severity.AtOwnRisk)    // risky to skip in DTO
            .WithLayerSeverity("entity", Severity.NotRecommended); // nice-to-have in DB

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is recommended.")
            .WithSeverity(Severity.AtOwnRisk)
            .WithLayerSeverity("api", Severity.AtOwnRisk)
            .WithLayerSeverity("entity", Severity.NotRecommended);
    }
}
```

When you call `validator.Validate(instance)`, the layer is automatically resolved from the `[ValidationLayer]` attribute on the model type. No additional configuration needed.

### Explicit Context

Override the layer at runtime:

```csharp
var context = ValidationContext.ForLayer("entity");
var result = validator.Validate(instance, context);
// Uses "entity" layer severities regardless of the type's attribute
```

---

## JSON-Based Validation

### Schema Overview

Define rules in a JSON file that both C# and TypeScript can consume:

```json
{
  "$schema": "https://common-validation/schema/v1.json",
  "type": "Invoice",
  "properties": {
    "invoiceNumber": {
      "rules": [
        {
          "validator": "notEmpty",
          "message": "Invoice number is required.",
          "severity": "forbidden"
        },
        {
          "validator": "matches",
          "params": { "pattern": "^INV-\\d{6}$" },
          "message": "Must match format INV-XXXXXX.",
          "severity": "forbidden"
        }
      ]
    },
    "amount": {
      "rules": [
        {
          "validator": "greaterThan",
          "params": { "value": 0 },
          "message": "Amount must be positive.",
          "severity": "forbidden",
          "layers": {
            "api": "forbidden",
            "entity": "atOwnRisk"
          }
        }
      ]
    }
  }
}
```

Available validator types in JSON: `notNull`, `null`, `notEmpty`, `empty`, `maxLength`, `minLength`, `length`, `email`, `phone`, `matches`, `equal`, `notEqual`, `greaterThan`, `greaterThanOrEqual`, `lessThan`, `lessThanOrEqual`, `inclusiveBetween`.

### Loading Definitions

```csharp
using Common.Validation.Json;

var loader = new JsonValidationDefinitionLoader();

// From a file
var definition = loader.LoadFromFile("Invoice.validation.json");

// From a JSON string (e.g., fetched from an API or embedded resource)
var definition = loader.Load(jsonString);

// From all *.validation.json files in a directory
var definitions = loader.LoadFromDirectory("Validations/");

// Create a validator from the definition
var validator = new JsonValidator<Invoice>(definition);
var result = validator.Validate(invoice);
```

### Custom Validator Types

Extend the registry with domain-specific validators:

```csharp
using Common.Validation.Json.Registry;

var registry = new ValidatorTypeRegistry();

// Register a custom "iban" validator
registry.Register("iban", parameters =>
{
    return new DelegatePropertyCheck(value =>
        value is string s && s.Length >= 15 && s.Length <= 34 && s.StartsWith("PL"));
});

// Use it
var validator = new JsonValidator<BankAccount>(definition, registry);
```

To implement `IPropertyCheck`:

```csharp
public class IbanCheck : IPropertyCheck
{
    public bool IsValid(object? value)
    {
        return value is string iban
            && iban.Length is >= 15 and <= 34
            && char.IsLetter(iban[0])
            && char.IsLetter(iban[1]);
    }
}
```

---

## Dependency Injection

### Basic Setup

```csharp
using Common.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommonValidation(options =>
{
    options.DefaultCascadeMode = CascadeMode.Continue;
    options.DefaultLayer = "api";
    options.JsonDefinitionPaths.Add("Validations/");
});
```

### Assembly Scanning

Automatically discover and register all validators in an assembly:

```csharp
// Scan a specific assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Or use a marker type
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

// Control lifetime (default: Transient)
builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);
```

### Validator Factory

Resolve validators at runtime when you don't know the type at compile time:

```csharp
public class ValidationMiddleware
{
    private readonly IValidatorFactory _factory;

    public ValidationMiddleware(IValidatorFactory factory)
    {
        _factory = factory;
    }

    public ValidationResult ValidateModel<T>(T model) where T : class
    {
        var validator = _factory.GetValidator<T>();
        if (validator is null)
            return new ValidationResult(); // no validator registered, pass through

        return validator.Validate(model);
    }
}
```

---

## Property-Level Validation

You can validate a single property instead of the entire object. This is useful for:

- **Real-time field validation** (e.g., on blur) without re-running rules for untouched fields
- **Partial updates** where only certain fields changed
- **Performance** when the object has many properties but you care about one

### Usage

Use the `ValidateProperty` extension method on any `IValidator<T>`:

```csharp
using Common.Validation.Extensions;

var validator = new CreateOrderValidator();
var order = new CreateOrderRequest { CustomerName = "", Email = "bad", Total = -5 };

// Validate only the Email field
var emailResult = validator.ValidateProperty(order, x => x.Email);

if (emailResult.IsValid)
    Console.WriteLine("Email is valid");
else
    foreach (var e in emailResult.Errors)
        Console.WriteLine($"{e.PropertyName}: {e.ErrorMessage}");
```

### With Validation Context

Property-level validation respects the same layer and context as full validation:

```csharp
var context = ValidationContext.ForLayer("entity");
var result = validator.ValidateProperty(model, x => x.FirstName, context);
```

### Supported Validators

- **AbstractValidator&lt;T&gt;** – Validates only the rules defined for the specified property.
- **JsonValidator&lt;T&gt;** – Validates only the rules from the JSON definition for that property.
- **Other IValidator&lt;T&gt;** – Falls back to full validation and filters the result by property name (less efficient but works).

### JSON-based property validation

`JsonValidator<T>` supports property-level validation with the same rules defined in your JSON definition. This is useful when rules come from configuration or are shared with a TypeScript frontend:

```csharp
using Common.Validation.Extensions;
using Common.Validation.Json;

// Load definition (e.g. from PersonalData.validation.json)
var definition = "PersonalData.validation.json".LoadFromFile();
var validator = new JsonValidator<PersonalData>(definition);

var model = new PersonalData
{
    FirstName = "Jan",
    LastName = "Nowak",
    Email = "invalid-email",  // Invalid
    Citizenship = "PL",
    TaxResidency = ""        // Not recommended
};

// Validate only the Email field
var emailResult = validator.ValidateProperty(model, x => x.Email);

if (!emailResult.IsValid)
    foreach (var e in emailResult.Errors)
        Console.WriteLine($"{e.PropertyName}: {e.ErrorMessage}");
// Output: Email: Invalid email format.
```

Layer context is respected when validating a single property:

```csharp
var context = ValidationContext.ForLayer("entity");
var result = validator.ValidateProperty(model, x => x.FirstName, context);
// Uses "entity" layer severities from the JSON definition
```

### When to Use

| Scenario | Use |
|----------|-----|
| Form field blur / `onChange` | `ValidateProperty` |
| Submit button clicked | `Validate` |
| API receives partial PATCH | `ValidateProperty` per changed field |
| Full model save | `Validate` |

---

## Standalone Value Validation

Sometimes you need to validate a value on its own — without embedding it in a model class, without an `AbstractValidator<T>`, and without any parent object at all. Common.Validation provides a standalone value validation system that mirrors the full fluent API but operates directly on a single value.

**Why standalone?**

- **Reusable rules.** Define "what a valid email looks like" once; use it across REST models, DTOs, Blazor form fields, and TypeScript.
- **No container coupling.** A phone number validator should not care whether the phone lives on a `Person`, an `Order`, or a raw `string` from a form input.
- **Client-side friendly.** Standalone validators map naturally to single-field validation in Blazor or TypeScript, where you often receive a raw value rather than a full object.
- **Composable.** Standalone validators can be invoked inside object validators via `.Must()`, or composed with `ValidationResult.Combine()`.

### Class-Based Value Validators

Inherit from `ValueValidator<TProperty>` to create a reusable, named validator:

```csharp
using Common.Validation.Core;
using Common.Validation.Extensions;

public class EmailValidator : ValueValidator<string>
{
    public EmailValidator() : base("Email")
    {
        Check()
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Must be a valid email address.")
            .MaxLength(255).WithMessage("Email is too long.");
    }
}
```

Use it — no parent object, no model class:

```csharp
var validator = new EmailValidator();

var result = validator.Validate("user@example.com");
// result.IsValid == true

var invalid = validator.Validate("not-an-email");
// invalid.IsValid == false
// invalid.Errors[0].ErrorMessage == "Must be a valid email address."
// invalid.Errors[0].PropertyName == "Email"
```

You can define validators for any type:

```csharp
public class AgeValidator : ValueValidator<int>
{
    public AgeValidator() : base("Age")
    {
        Check()
            .GreaterThanOrEqual(0).WithMessage("Age cannot be negative.")
            .LessThanOrEqual(150).WithMessage("Age is unrealistic.");
    }
}

public class PasswordStrengthValidator : ValueValidator<string>
{
    public PasswordStrengthValidator() : base("Password")
    {
        Check()
            .NotEmpty().WithMessage("Password is required.")
            .MinLength(8).WithMessage("Must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Must contain an uppercase letter.")
                .WithSeverity(Severity.AtOwnRisk)
            .Matches(@"\d").WithMessage("Must contain a digit.")
                .WithSeverity(Severity.AtOwnRisk);
    }
}
```

### Inline Factory

For quick, ad-hoc validation without a dedicated class, use `ValueValidator.Create<T>()`:

```csharp
var phoneValidator = ValueValidator.Create<string>(
    configure: b => b
        .NotEmpty().WithMessage("Phone number is required.")
        .PhoneNumber().WithMessage("Not a valid phone format."),
    propertyName: "Phone");

var result = phoneValidator.Validate("+48 123 456 789");
// result.IsValid == true
```

```csharp
var percentValidator = ValueValidator.Create<decimal>(
    configure: b => b
        .InclusiveBetween(0m, 100m).WithMessage("Must be a percentage (0–100)."),
    propertyName: "Discount");

var result = percentValidator.Validate(105m);
// result.IsValid == false
```

When you omit `propertyName`, it defaults to the type name (e.g., `"String"`, `"Int32"`):

```csharp
var notEmpty = ValueValidator.Create<string>(b => b.NotEmpty());
var result = notEmpty.Validate("");
// result.Errors[0].PropertyName == "String"
```

### Standalone Rules & Modifiers

Standalone validators support the same rules and modifiers as object validators:

**Common rules** (any type): `.NotNull()`, `.Null()`, `.NotEmpty()`, `.Empty()`, `.Equal(value)`, `.NotEqual(value)`, `.Must(predicate, msg)`

**String rules:** `.MinLength(n)`, `.MaxLength(n)`, `.Length(min, max)`, `.Matches(pattern)`, `.EmailAddress()`, `.PhoneNumber()`

**Comparison rules** (`IComparable<T>`): `.GreaterThan(n)`, `.GreaterThanOrEqual(n)`, `.LessThan(n)`, `.LessThanOrEqual(n)`, `.InclusiveBetween(a, b)`

**Modifiers:** `.WithMessage("...")`, `.WithErrorCode("...")`, `.WithSeverity(Severity.X)`, `.WithLayerSeverity("api", Severity.X)`, `.Cascade(CascadeMode.StopOnFirstFailure)`

### Layer Support in Standalone Mode

Standalone validators fully support multi-layer severity overrides:

```csharp
public class TaxIdValidator : ValueValidator<string>
{
    public TaxIdValidator() : base("TaxId")
    {
        Check()
            .NotEmpty().WithMessage("Tax ID is required.")
            .WithSeverity(Severity.Forbidden)
            .WithLayerSeverity("api", Severity.Forbidden)
            .WithLayerSeverity("dto", Severity.AtOwnRisk)
            .WithLayerSeverity("entity", Severity.NotRecommended);
    }
}

var validator = new TaxIdValidator();

// Default severity: Forbidden
var defaultResult = validator.Validate("");
// defaultResult.Errors[0].Severity == Severity.Forbidden

// API layer: still Forbidden
var apiResult = validator.Validate("", ValidationContext.ForLayer("api"));
// apiResult.Errors[0].Severity == Severity.Forbidden

// Entity layer: just a recommendation
var entityResult = validator.Validate("", ValidationContext.ForLayer("entity"));
// entityResult.Errors[0].Severity == Severity.NotRecommended
```

### Value-Based Conditions

In object validators, `When()` / `Unless()` receive the parent instance. In standalone mode, they receive the value itself:

```csharp
var validator = ValueValidator.Create<string>(
    configure: b => b
        // Only validate length when the value is not null
        .When(value => value is not null)
        .MinLength(3).WithMessage("Too short — must be at least 3 characters.")
        .MaxLength(50).WithMessage("Too long — at most 50 characters."),
    propertyName: "Code");

validator.Validate(null!).IsValid;   // true  — condition skipped
validator.Validate("ab").IsValid;    // false — MinLength fails
validator.Validate("abc").IsValid;   // true
```

```csharp
var validator = ValueValidator.Create<string>(
    configure: b => b
        // Skip validation when the value is null (optional field)
        .Unless(value => value is null)
        .EmailAddress().WithMessage("Not a valid email."),
    propertyName: "SecondaryEmail");

validator.Validate(null!).IsValid;          // true  — skipped
validator.Validate("bad").IsValid;          // false — checked
validator.Validate("a@b.com").IsValid;      // true  — valid
```

### Multiple Check Chains

Call `Check()` multiple times inside a class-based validator to create independent rule chains. Combined with `CascadeMode.StopOnFirstFailure` on the validator, you can control the flow precisely:

```csharp
public class StrictCodeValidator : ValueValidator<string>
{
    public StrictCodeValidator() : base("Code")
    {
        CascadeMode = CascadeMode.StopOnFirstFailure;

        Check().NotNull().WithMessage("Code is required.");
        Check().MinLength(5).WithMessage("Code is too short.");
        Check().Matches(@"^[A-Z0-9]+$").WithMessage("Code must be uppercase alphanumeric.");
    }
}

var validator = new StrictCodeValidator();
var result = validator.Validate(null!);
// result.Errors.Count == 1  (stopped after "Code is required.")
```

### Non-Generic Interface

Like `IValidator`, the standalone system provides a non-generic `IValueValidator` interface for DI and runtime scenarios:

```csharp
IValueValidator validator = new EmailValidator();

Console.WriteLine(validator.ValidatedType);        // System.String
var result = validator.Validate("test@test.com");   // works with object?
```

### When to Use Standalone vs Object Validation

| Scenario | Approach |
|----------|----------|
| Validate a form field individually | **Standalone** (`ValueValidator<string>`) |
| Validate an API request model | **Object** (`AbstractValidator<T>`) |
| Reusable "is this a valid email?" check | **Standalone** (`EmailValidator`) |
| Cross-property rule (end > start date) | **Object** (`.Must((obj, val) => ...)`) |
| Blazor `@onblur` on a single `<input>` | **Standalone** or `ValidateProperty` |
| TypeScript single-field validation | **Standalone** (same mental model) |
| Full model save / submit | **Object** (`AbstractValidator<T>`) |
| Shared validation logic between models | **Standalone** (compose via `.Must()` or `Combine`) |

### Composing Standalone with Object Validators

Reuse a standalone validator inside an object validator:

```csharp
var emailValidator = new EmailValidator();
var phoneValidator = ValueValidator.Create<string>(
    b => b.NotEmpty().PhoneNumber(),
    propertyName: "Phone");

public class ContactValidator : AbstractValidator<Contact>
{
    public ContactValidator()
    {
        RuleFor(x => x.Email)
            .Must(value => emailValidator.Validate(value).IsValid,
                  "Must be a valid email address.");

        RuleFor(x => x.Phone)
            .Must(value => phoneValidator.Validate(value).IsValid,
                  "Must be a valid phone number.");
    }
}
```

Or merge results explicitly:

```csharp
var emailResult = emailValidator.Validate(formData.Email);
var phoneResult = phoneValidator.Validate(formData.Phone);
var combined = ValidationResult.Combine(emailResult, phoneResult);

if (combined.HasForbidden) { /* ... */ }
```

---

## Blazor Integration

### Components

**Validation summary** -- displays all failures grouped by severity:

```razor
@using Common.Validation.Blazor

<CvValidationSummary Result="@_validationResult" />
```

**Per-field messages** -- display failures for a specific property:

```razor
<InputText @bind-Value="_model.Email" class="form-control" />
<CvValidationMessage Result="@_result" PropertyName="Email" />
```

Both components use CSS classes for severity-based styling:
- `.cv-forbidden` -- red
- `.cv-at-own-risk` -- amber
- `.cv-not-recommended` -- gray

### EditContext Integration

Hook into Blazor's built-in form validation:

```csharp
@using Common.Validation.Blazor

<EditForm EditContext="@_editContext" OnSubmit="HandleSubmit">
    <InputText @bind-Value="_model.Name" />
    <ValidationMessage For="@(() => _model.Name)" />

    <button type="submit">Save</button>
</EditForm>

@code {
    private EditContext _editContext = default!;
    private MyModel _model = new();

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
        _editContext.AddCommonValidation(new MyValidator());
    }
}
```

---

## TypeScript Client

The TypeScript package consumes the same JSON definitions as the C# backend:

```typescript
import { Validator } from 'common-validation';
import type { ValidationDefinition } from 'common-validation';

// Load the same JSON definition used by the backend
const definition: ValidationDefinition = await fetch('/api/validations/invoice')
  .then(r => r.json());

const validator = new Validator(definition);

const result = validator.validate({
  invoiceNumber: '',
  amount: -5
});

if (result.hasForbidden) {
  result.errors
    .filter(e => e.severity === 'forbidden')
    .forEach(e => showFieldError(e.propertyName, e.errorMessage));
}
```

Layer-aware validation works the same way:

```typescript
// Validate with a specific layer
const result = validator.validate(formData, 'api');
```

Register custom validators on the client side:

```typescript
import { Validator } from 'common-validation';

const validator = new Validator(definition, {
  iban: () => (value) =>
    typeof value === 'string' && value.length >= 15 && value.length <= 34
});
```

---

## Architecture

```
common.validation/
  src/
    Common.Validation/                   # Core NuGet package
      Core/                              #   IValidator, AbstractValidator, Severity,
                                         #   ValidationResult, CascadeMode, ValidationContext
                                         #   IValueValidator, ValueValidator (standalone)
      Rules/                             #   IRuleBuilder, IValidationRule, PropertyRule
                                         #   IValueRuleBuilder, IValueValidationRule, ValueRule (standalone)
      Extensions/                        #   Fluent API (NotEmpty, MaxLength, WithSeverity, etc.)
                                         #   Standalone extensions (*ValueRule* variants)
      Layers/                            #   ValidationLayerAttribute
      Json/                              #   JsonValidator, definition models, loader
        Registry/                        #   IValidatorTypeRegistry, built-in checks
      DependencyInjection/               #   AddCommonValidation, IValidatorFactory
    Common.Validation.Blazor/            # Blazor NuGet package (Razor Class Library)
                                         #   CvValidationSummary, CvValidationMessage,
                                         #   EditContext extensions, CSS isolation
  client/
    common-validation/                   # TypeScript npm package
      src/                              #   Validator, types, built-in rules
      schema/                           #   JSON Schema for IDE autocompletion
  tests/
    Common.Validation.Tests/             # xUnit tests (213 tests)
  demo/
    Common.Validation.Demo/              # Console demo (fluent, layers, JSON, DI)
    Common.Validation.Demo.Blazor/       # Blazor interactive demo
```

---

## Use Cases

### REST API Input Validation

Validate incoming requests in a controller or middleware, rejecting `Forbidden` failures and passing through `AtOwnRisk` / `NotRecommended` as response headers or metadata.

```csharp
[HttpPost]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    var result = _validator.Validate(request);

    if (result.HasForbidden)
        return BadRequest(result.BySeverity(Severity.Forbidden)
            .Select(e => new { e.PropertyName, e.ErrorMessage }));

    if (result.HasAtOwnRisk)
        Response.Headers.Append("X-Validation-Warnings",
            string.Join("; ", result.BySeverity(Severity.AtOwnRisk).Select(e => e.ErrorMessage)));

    return Ok(_service.CreateUser(request));
}
```

### Multi-Tenant Configurable Validation

Load validation rules from a database or configuration API per tenant, using JSON definitions:

```csharp
builder.Services.AddCommonValidation(options =>
{
    options.JsonDefinitionPaths.Add($"Validations/{tenantId}/");
});
```

### Form Wizard with Progressive Strictness

In a multi-step form, earlier steps use relaxed severity; the final step enforces everything:

```csharp
// Step 1: only show hints
var step1Context = ValidationContext.ForLayer("draft");

// Step 2: warn about risks
var step2Context = ValidationContext.ForLayer("review");

// Final submit: block on forbidden
var submitContext = ValidationContext.ForLayer("api");
```

### Shared Client-Server Validation

Define rules in JSON, serve them from the API, validate on both sides:

```
Backend:  JsonValidator<T>(definition)          --> ValidationResult
Frontend: new Validator(definition).validate(t) --> ValidationResult
```

Both produce the same failures for the same input, ensuring UI error messages match server-side enforcement.

### PATCH API with per-field JSON validation

When handling partial updates (PATCH), validate only the fields that changed using rules from a JSON definition:

```csharp
[HttpPatch("{id}")]
public IActionResult UpdateUser(Guid id, [FromBody] Dictionary<string, object?> changedFields)
{
    var target = _repository.Get(id);
    var update = MapToUpdate(target, changedFields);

    var definition = "User.validation.json".LoadFromFile();
    var validator = new JsonValidator<UserUpdate>(definition);

    foreach (var (key, _) in changedFields)
    {
        var result = key switch
        {
            "email" => validator.ValidateProperty(update, u => u.Email),
            "firstName" => validator.ValidateProperty(update, u => u.FirstName),
            _ => new ValidationResult()
        };
        if (result.HasForbidden)
            return BadRequest(result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
    }

    return Ok(_service.Update(target, update));
}
```

This avoids validating untouched fields and keeps rules consistent with the shared JSON definition.

### Reusable Field Validators Across Multiple Models

Define validation once for a data concept, reuse it in every model that contains it:

```csharp
// Define once
public class EmailValidator : ValueValidator<string>
{
    public EmailValidator() : base("Email")
    {
        Check()
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Must be a valid email address.")
            .MaxLength(255);
    }
}

public class PhoneValidator : ValueValidator<string>
{
    public PhoneValidator() : base("Phone")
    {
        Check()
            .NotEmpty().WithMessage("Phone is required.")
            .PhoneNumber().WithMessage("Must be a valid phone number.");
    }
}

// Reuse in object validators
var email = new EmailValidator();
var phone = new PhoneValidator();

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Email)
            .Must(v => email.Validate(v).IsValid, "Invalid email.");

        RuleFor(x => x.Phone)
            .Must(v => phone.Validate(v).IsValid, "Invalid phone.");
    }
}

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.WorkEmail)
            .Must(v => email.Validate(v).IsValid, "Invalid work email.");
    }
}
```

### Domain Entity Invariants

Validate invariants within domain entities themselves, using the non-generic `IValidator` interface for polymorphic dispatch:

```csharp
public abstract class Entity
{
    public ValidationResult Validate(IValidatorFactory factory)
    {
        var validator = factory.GetValidator(GetType());
        return validator?.Validate(this) ?? new ValidationResult();
    }
}
```

### Composite Validation Across Aggregates

Validate an entire aggregate root by merging results from child validators:

```csharp
var orderResult = _orderValidator.Validate(order);
var itemResults = order.Items.Select(item => _itemValidator.Validate(item));

var combined = ValidationResult.Combine(
    new[] { orderResult }.Concat(itemResults).ToArray());

if (combined.HasForbidden) { /* ... */ }
```

---

## Roadmap

The project is under active development. Planned directions include:

- **Async validation** -- `ValidateAsync` for rules that need I/O (e.g., checking uniqueness against a database). The `IValidator<T>` interface is designed to support this.
- **Localization** -- message templates with placeholders and resource-file integration for multi-language support.
- **Source generators** -- compile-time validator discovery and AOT-compatible JSON serialization, eliminating reflection overhead.
- **Validation profiles** -- named rule sets within a single validator, selectable at validation time (e.g., "create" vs. "update" profiles).
- **OpenAPI integration** -- auto-generate `x-validation` metadata in Swagger/OpenAPI specs from JSON definitions.
- **React / Vue adapters** -- framework-specific wrappers around the TypeScript client for seamless form integration.
- **Rule composition** -- `Include()` and `InheritRulesFrom()` for composing validators from reusable fragments. Automatic bridging of `IValueValidator<T>` into `AbstractValidator<T>` rules via a dedicated extension method.
- **Diagnostic analyzers** -- Roslyn analyzers to catch common mistakes at compile time (e.g., forgetting `.WithMessage()` after a rule).

---

## License

This project is licensed under the **GNU Affero General Public License v3.0** (AGPL-3.0-or-later).
See [LICENSE.txt](LICENSE.txt) for the full text.
