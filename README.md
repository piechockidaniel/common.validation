# Atlas.Common.Validation

Interoperacyjny, wielowarstwowy framework walidacyjny dla .NET 10 i TypeScript. Definiuj reguły walidacji raz -- w C# lub JSON -- i egzekwuj je w modelach API, DTO, encjach bazodanowych oraz formularzach frontendowych z kontrolą istotności na poziomie warstw.

## Spis treści

- [Kluczowe koncepcje](#kluczowe-koncepcje)
- [Pierwsze kroki](#pierwsze-kroki)
  - [Instalacja](#instalacja)
  - [Twój pierwszy walidator](#twój-pierwszy-walidator)
  - [Interpretacja wyników](#interpretacja-wyników)
- [Fluent API -- dokumentacja](#fluent-api----dokumentacja)
  - [Wbudowane reguły](#wbudowane-reguły)
  - [Reguły niestandardowe](#reguły-niestandardowe)
  - [Warunki](#warunki)
  - [Tryb kaskadowy](#tryb-kaskadowy)
- [Wielowarstwowa istotność](#wielowarstwowa-istotność)
  - [Problem](#problem)
  - [Atrybuty warstw](#atrybuty-warstw)
  - [Reguły uwzględniające warstwy](#reguły-uwzględniające-warstwy)
  - [Jawny kontekst](#jawny-kontekst)
- [Walidacja oparta na JSON](#walidacja-oparta-na-json)
  - [Przegląd schematu](#przegląd-schematu)
  - [Wczytywanie definicji](#wczytywanie-definicji)
  - [Niestandardowe typy walidatorów](#niestandardowe-typy-walidatorów)
- [Wstrzykiwanie zależności](#wstrzykiwanie-zależności)
  - [Podstawowa konfiguracja](#podstawowa-konfiguracja)
  - [Skanowanie assembly](#skanowanie-assembly)
  - [Fabryka walidatorów](#fabryka-walidatorów)
- [Walidacja na poziomie właściwości](#walidacja-na-poziomie-właściwości)
- [Samodzielna walidacja wartości](#samodzielna-walidacja-wartości)
  - [Walidatory wartości oparte na klasach](#walidatory-wartości-oparte-na-klasach)
  - [Fabryka inline](#fabryka-inline)
  - [Reguły i modyfikatory w trybie samodzielnym](#reguły-i-modyfikatory-w-trybie-samodzielnym)
  - [Obsługa warstw w trybie samodzielnym](#obsługa-warstw-w-trybie-samodzielnym)
  - [Warunki oparte na wartości](#warunki-oparte-na-wartości)
  - [Kiedy używać walidacji samodzielnej a obiektowej](#kiedy-używać-walidacji-samodzielnej-a-obiektowej)
- [Integracja z Blazor](#integracja-z-blazor)
  - [Komponenty](#komponenty)
  - [Integracja z EditContext](#integracja-z-editcontext)
- [Klient TypeScript](#klient-typescript)
- [Architektura](#architektura)
- [Przypadki użycia](#przypadki-użycia)
- [Plan rozwoju](#plan-rozwoju)
- [Licencja](#licencja)

---

## Kluczowe koncepcje

Common.Validation opiera się na trzech ideach, które wyróżniają go spośród innych bibliotek walidacyjnych:

**Istotność nie jest binarna.** Niepowodzenia walidacji to nie tylko "poprawne" lub "niepoprawne". Każde niepowodzenie niesie ze sobą poziom istotności (`Severity`):

| Istotność | Znaczenie | Typowa akcja |
|---|---|---|
| `Forbidden` | Wartość jest nieprawidłowa. Operacja nie może być kontynuowana. | Zablokuj żądanie. |
| `AtOwnRisk` | Wartość jest ryzykowna. Wywołujący przyjmuje odpowiedzialność. | Ostrzeż użytkownika, zaloguj, kontynuuj. |
| `NotRecommended` | Wartość jest technicznie poprawna, ale nie idealna. | Wyświetl informacyjną wskazówkę. |

**Warstwy zmieniają istotność.** Ta sama reguła walidacji może generować różne poziomy istotności w zależności od tego, gdzie jest uruchamiana. Pominięcie numeru telefonu może być `Forbidden` na warstwie API, ale jedynie `NotRecommended` w encji bazodanowej.

**Jedna definicja, wiele środowisk uruchomieniowych.** Reguły zdefiniowane we wspólnym schemacie JSON są konsumowane zarówno przez backend C#, jak i frontend TypeScript, zapewniając spójność klient-serwer bez duplikowania logiki.

---

## Pierwsze kroki

### Instalacja

Dodaj pakiet NuGet do swojego projektu:

```bash
dotnet add package Atlas.Common.Validation
```

Dla komponentów Blazor:

```bash
dotnet add package Atlas.Common.Validation.Blazor
```

Dla TypeScript/JavaScript:

```bash
npm install atlas-common-validation
```

### Twój pierwszy walidator

Utwórz model i walidator:

```csharp
using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Extensions;

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
            .NotEmpty().WithMessage("Nazwa klienta jest wymagana.")
            .MaxLength(200).WithMessage("Nazwa jest za długa.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .EmailAddress().WithMessage("Nieprawidłowy adres email.");

        RuleFor(x => x.Total)
            .GreaterThan(0m).WithMessage("Suma zamówienia musi być dodatnia.");

        RuleFor(x => x.PromoCode)
            .MaxLength(20).WithMessage("Kod promocyjny jest za długi.")
            .WithSeverity(Severity.NotRecommended);
    }
}
```

Użycie:

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
    // Blokada: obowiązkowe pola są puste lub nieprawidłowe
    foreach (var error in result.BySeverity(Severity.Forbidden))
        Console.WriteLine($"  BŁĄD: {error.PropertyName} - {error.ErrorMessage}");
}
```

### Interpretacja wyników

`ValidationResult` udostępnia kilka sposobów inspekcji niepowodzeń:

```csharp
result.IsValid              // true jeśli brak niepowodzeń
result.HasForbidden         // jakiekolwiek błędy blokujące?
result.HasAtOwnRisk         // jakiekolwiek ostrzeżenia o ryzyku?
result.HasNotRecommended    // jakiekolwiek miękkie wskazówki?
result.Errors               // wszystkie niepowodzenia jako IReadOnlyList<ValidationFailure>
result.BySeverity(severity) // filtrowanie według poziomu istotności
```

Możesz łączyć wyniki z wielu walidatorów:

```csharp
var combined = ValidationResult.Combine(
    addressValidator.Validate(order.Address),
    paymentValidator.Validate(order.Payment)
);
```

---

## Fluent API -- dokumentacja

### Wbudowane reguły

**Reguły ogólne** (dowolny typ właściwości):

| Metoda | Opis |
|---|---|
| `.NotNull()` | Wartość nie może być null |
| `.Null()` | Wartość musi być null |
| `.NotEmpty()` | String nie null/whitespace, kolekcja niepusta |
| `.Empty()` | Odwrotność NotEmpty |
| `.Equal(value)` | Musi być równa podanej wartości |
| `.NotEqual(value)` | Nie może być równa podanej wartości |
| `.Must(predicate, msg)` | Niestandardowy predykat |

**Reguły tekstowe:**

| Metoda | Opis |
|---|---|
| `.MinLength(n)` | Co najmniej n znaków |
| `.MaxLength(n)` | Co najwyżej n znaków |
| `.Length(min, max)` | Między min a max znaków |
| `.Matches(pattern)` | Pasuje do wzorca regex |
| `.EmailAddress()` | Prawidłowy format email |
| `.PhoneNumber()` | Prawidłowy format telefonu |

**Reguły porównawcze** (dla `IComparable<T>`):

| Metoda | Opis |
|---|---|
| `.GreaterThan(n)` | Ściśle większa niż n |
| `.GreaterThanOrEqual(n)` | Większa lub równa n |
| `.LessThan(n)` | Ściśle mniejsza niż n |
| `.LessThanOrEqual(n)` | Mniejsza lub równa n |
| `.InclusiveBetween(a, b)` | Między a i b (włącznie) |

**Modyfikatory** (łańcuchowo po dowolnej regule):

| Metoda | Opis |
|---|---|
| `.WithMessage("...")` | Niestandardowy komunikat błędu |
| `.WithErrorCode("...")` | Programistyczny kod błędu |
| `.WithSeverity(Severity.X)` | Ustaw domyślną istotność |
| `.WithLayerSeverity("api", Severity.X)` | Istotność specyficzna dla warstwy |
| `.Cascade(CascadeMode.StopOnFirstFailure)` | Zatrzymaj przy pierwszym niepowodzeniu dla tej właściwości |

### Reguły niestandardowe

Użyj `.Must()` dla predykatów inline:

```csharp
RuleFor(x => x.StartDate)
    .Must(date => date >= DateTime.Today, "Data rozpoczęcia musi być w przyszłości.");
```

Dostęp do obiektu nadrzędnego w celu walidacji międzywłaściwościowej:

```csharp
RuleFor(x => x.EndDate)
    .Must((order, endDate) => endDate > order.StartDate,
          "Data zakończenia musi być po dacie rozpoczęcia.");
```

### Warunki

Stosuj reguły warunkowo:

```csharp
RuleFor(x => x.CompanyName)
    .When(x => x.CustomerType == CustomerType.Business)
    .NotEmpty().WithMessage("Nazwa firmy jest wymagana dla klientów biznesowych.");

RuleFor(x => x.PersonalId)
    .Unless(x => x.CustomerType == CustomerType.Business)
    .NotEmpty().WithMessage("PESEL jest wymagany dla klientów indywidualnych.");
```

### Tryb kaskadowy

Kontroluj, czy walidacja kontynuuje po niepowodzeniu:

```csharp
// Poziom walidatora: zatrzymaj po pierwszej regule z niepowodzeniami
public class StrictValidator : AbstractValidator<Order>
{
    public StrictValidator()
    {
        CascadeMode = CascadeMode.StopOnFirstFailure;
        // ...
    }
}

// Poziom właściwości: zatrzymaj sprawdzanie tej właściwości po pierwszym niepowodzeniu
RuleFor(x => x.Email)
    .Cascade(CascadeMode.StopOnFirstFailure)
    .NotEmpty().WithMessage("Wymagane.")
    .EmailAddress().WithMessage("Nieprawidłowy format.");
    // Jeśli NotEmpty nie przejdzie, EmailAddress zostanie pominięte
```

---

## Wielowarstwowa istotność

### Problem

W typowej aplikacji korporacyjnej te same dane przechodzą przez wiele warstw:

```
Formularz przeglądarki  -->  Kontroler API  -->  Warstwa serwisowa  -->  Encja bazodanowa
```

Każda warstwa ma inne ograniczenia. Adres email może być:
- **Forbidden** do pominięcia na warstwie API (nie można utworzyć użytkownika bez niego)
- **AtOwnRisk** na poziomie DTO (częściowe aktualizacje mogą go pomijać)
- **NotRecommended** na poziomie encji (starsze rekordy mogą go nie mieć)

Atlas.Common.Validation rozwiązuje to za pomocą nadpisywania istotności na poziomie warstw.

### Atrybuty warstw

Oznacz swoje modele warstwą, do której należą:

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

Atrybut jest dziedziczony, więc atrybuty klas bazowych są przekazywane do klas pochodnych:

```csharp
[ValidationLayer("api")]
public abstract class ApiModelBase { }

public class UserApiModel : ApiModelBase { }
// UserApiModel automatycznie rozpoznaje warstwę "api"
```

### Reguły uwzględniające warstwy

Definiuj reguły raz z nadpisywaniem istotności dla poszczególnych warstw:

```csharp
public class UserValidator : AbstractValidator<UserApiModel>
{
    public UserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .WithSeverity(Severity.Forbidden)               // domyślna
            .WithLayerSeverity("api", Severity.Forbidden)    // obowiązkowy w API
            .WithLayerSeverity("dto", Severity.AtOwnRisk)    // ryzykowne pominięcie w DTO
            .WithLayerSeverity("entity", Severity.NotRecommended); // mile widziany w BD

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon jest zalecany.")
            .WithSeverity(Severity.AtOwnRisk)
            .WithLayerSeverity("api", Severity.AtOwnRisk)
            .WithLayerSeverity("entity", Severity.NotRecommended);
    }
}
```

Gdy wywołujesz `validator.Validate(instance)`, warstwa jest automatycznie rozpoznawana na podstawie atrybutu `[ValidationLayer]` na typie modelu. Dodatkowa konfiguracja nie jest wymagana.

### Jawny kontekst

Nadpisz warstwę w czasie wykonywania:

```csharp
var context = ValidationContext.ForLayer("entity");
var result = validator.Validate(instance, context);
// Używa istotności warstwy "entity" niezależnie od atrybutu typu
```

---

## Walidacja oparta na JSON

### Przegląd schematu

Definiuj reguły w pliku JSON, który może być konsumowany zarówno przez C#, jak i TypeScript:

```json
{
  "$schema": "https://common-validation/schema/v1.json",
  "type": "Invoice",
  "properties": {
    "invoiceNumber": {
      "rules": [
        {
          "validator": "notEmpty",
          "message": "Numer faktury jest wymagany.",
          "severity": "forbidden"
        },
        {
          "validator": "matches",
          "params": { "pattern": "^INV-\\d{6}$" },
          "message": "Musi pasować do formatu INV-XXXXXX.",
          "severity": "forbidden"
        }
      ]
    },
    "amount": {
      "rules": [
        {
          "validator": "greaterThan",
          "params": { "value": 0 },
          "message": "Kwota musi być dodatnia.",
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

Dostępne typy walidatorów w JSON: `notNull`, `null`, `notEmpty`, `empty`, `maxLength`, `minLength`, `length`, `email`, `phone`, `matches`, `equal`, `notEqual`, `greaterThan`, `greaterThanOrEqual`, `lessThan`, `lessThanOrEqual`, `inclusiveBetween`.

### Wczytywanie definicji

```csharp
using Common.Validation.Json;

var loader = new JsonValidationDefinitionLoader();

// Z pliku
var definition = loader.LoadFromFile("Invoice.validation.json");

// Z ciągu JSON (np. pobranego z API lub zasobu osadzonego)
var definition = loader.Load(jsonString);

// Ze wszystkich plików *.validation.json w katalogu
var definitions = loader.LoadFromDirectory("Validations/");

// Utworzenie walidatora z definicji
var validator = new JsonValidator<Invoice>(definition);
var result = validator.Validate(invoice);
```

### Niestandardowe typy walidatorów

Rozszerz rejestr o walidatory specyficzne dla domeny:

```csharp
using Common.Validation.Json.Registry;

var registry = new ValidatorTypeRegistry();

// Rejestracja niestandardowego walidatora "iban"
registry.Register("iban", parameters =>
{
    return new DelegatePropertyCheck(value =>
        value is string s && s.Length >= 15 && s.Length <= 34 && s.StartsWith("PL"));
});

// Użycie
var validator = new JsonValidator<BankAccount>(definition, registry);
```

Implementacja `IPropertyCheck`:

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

## Wstrzykiwanie zależności

### Podstawowa konfiguracja

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

### Skanowanie assembly

Automatyczne wykrywanie i rejestracja wszystkich walidatorów w assembly:

```csharp
// Skanuj konkretne assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Lub użyj typu znacznikowego
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

// Kontrola czasu życia (domyślnie: Transient)
builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);
```

### Fabryka walidatorów

Rozwiązywanie walidatorów w czasie wykonywania, gdy typ nie jest znany w czasie kompilacji:

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
            return new ValidationResult(); // brak zarejestrowanego walidatora, przepuść
        return validator.Validate(model);
    }
}
```

---

## Walidacja na poziomie właściwości

Możesz walidować pojedynczą właściwość zamiast całego obiektu. Jest to przydatne w przypadku:

- **Walidacji pól w czasie rzeczywistym** (np. przy utracie fokusu) bez ponownego uruchamiania reguł dla nietkniętych pól
- **Częściowych aktualizacji**, gdzie zmieniły się tylko niektóre pola
- **Wydajności**, gdy obiekt ma wiele właściwości, a interesuje Cię tylko jedna

### Użycie

Użyj metody rozszerzającej `ValidateProperty` na dowolnym `IValidator<T>`:

```csharp
using Common.Validation.Extensions;

var validator = new CreateOrderValidator();
var order = new CreateOrderRequest { CustomerName = "", Email = "bad", Total = -5 };

// Waliduj tylko pole Email
var emailResult = validator.ValidateProperty(order, x => x.Email);

if (emailResult.IsValid)
    Console.WriteLine("Email jest prawidłowy");
else
    foreach (var e in emailResult.Errors)
        Console.WriteLine($"{e.PropertyName}: {e.ErrorMessage}");
```

### Z kontekstem walidacji

Walidacja na poziomie właściwości respektuje te same warstwy i kontekst co pełna walidacja:

```csharp
var context = ValidationContext.ForLayer("entity");
var result = validator.ValidateProperty(model, x => x.FirstName, context);
```

### Obsługiwane walidatory

- **AbstractValidator&lt;T&gt;** -- Waliduje tylko reguły zdefiniowane dla wskazanej właściwości.
- **JsonValidator&lt;T&gt;** -- Waliduje tylko reguły z definicji JSON dla danej właściwości.
- **Inne IValidator&lt;T&gt;** -- Wykonuje pełną walidację i filtruje wynik po nazwie właściwości (mniej wydajne, ale działa).

### Walidacja właściwości oparta na JSON

`JsonValidator<T>` obsługuje walidację na poziomie właściwości z tymi samymi regułami zdefiniowanymi w definicji JSON. Jest to przydatne, gdy reguły pochodzą z konfiguracji lub są współdzielone z frontendem TypeScript:

```csharp
using Common.Validation.Extensions;
using Common.Validation.Json;

// Wczytaj definicję (np. z PersonalData.validation.json)
var definition = "PersonalData.validation.json".LoadFromFile();
var validator = new JsonValidator<PersonalData>(definition);

var model = new PersonalData
{
    FirstName = "Jan",
    LastName = "Nowak",
    Email = "nieprawidłowy-email",  // Nieprawidłowy
    Citizenship = "PL",
    TaxResidency = ""               // Niezalecane
};

// Waliduj tylko pole Email
var emailResult = validator.ValidateProperty(model, x => x.Email);

if (!emailResult.IsValid)
    foreach (var e in emailResult.Errors)
        Console.WriteLine($"{e.PropertyName}: {e.ErrorMessage}");
// Wynik: Email: Nieprawidłowy format email.
```

Kontekst warstwy jest respektowany przy walidacji pojedynczej właściwości:

```csharp
var context = ValidationContext.ForLayer("entity");
var result = validator.ValidateProperty(model, x => x.FirstName, context);
// Używa istotności warstwy "entity" z definicji JSON
```

### Kiedy używać

| Scenariusz | Użycie |
|----------|-----|
| Utrata fokusu pola / `onChange` | `ValidateProperty` |
| Kliknięcie przycisku wyślij | `Validate` |
| API otrzymuje częściowy PATCH | `ValidateProperty` dla każdego zmienionego pola |
| Pełny zapis modelu | `Validate` |

---

## Samodzielna walidacja wartości

Czasami potrzebujesz zwalidować wartość samodzielnie -- bez osadzania jej w klasie modelu, bez `AbstractValidator<T>` i bez jakiegokolwiek obiektu nadrzędnego. Common.Validation dostarcza samodzielny system walidacji wartości, który odzwierciedla pełne Fluent API, ale operuje bezpośrednio na pojedynczej wartości.

**Dlaczego samodzielna walidacja?**

- **Reguły wielokrotnego użytku.** Zdefiniuj "jak wygląda prawidłowy email" raz; używaj go w modelach REST, DTO, polach formularzy Blazor i TypeScript.
- **Brak sprzężenia z kontenerem.** Walidator numeru telefonu nie powinien wiedzieć, czy telefon należy do `Person`, `Order` czy surowego `string` z formularza.
- **Przyjazny dla klienta.** Samodzielne walidatory naturalnie mapują się na walidację pojedynczego pola w Blazor lub TypeScript, gdzie często otrzymujesz surową wartość zamiast pełnego obiektu.
- **Kompozycyjny.** Samodzielne walidatory można wywoływać wewnątrz walidatorów obiektowych przez `.Must()` lub komponować za pomocą `ValidationResult.Combine()`.

### Walidatory wartości oparte na klasach

Dziedzicz z `ValueValidator<TProperty>`, aby utworzyć wielokrotnie używalny, nazwany walidator:

```csharp
using Common.Validation.Core;
using Common.Validation.Extensions;

public class EmailValidator : ValueValidator<string>
{
    public EmailValidator() : base("Email")
    {
        Check()
            .NotEmpty().WithMessage("Email jest wymagany.")
            .EmailAddress().WithMessage("Musi być prawidłowym adresem email.")
            .MaxLength(255).WithMessage("Email jest za długi.");
    }
}
```

Użycie -- bez obiektu nadrzędnego, bez klasy modelu:

```csharp
var validator = new EmailValidator();

var result = validator.Validate("user@example.com");
// result.IsValid == true

var invalid = validator.Validate("nie-email");
// invalid.IsValid == false
// invalid.Errors[0].ErrorMessage == "Musi być prawidłowym adresem email."
// invalid.Errors[0].PropertyName == "Email"
```

Możesz definiować walidatory dla dowolnego typu:

```csharp
public class AgeValidator : ValueValidator<int>
{
    public AgeValidator() : base("Age")
    {
        Check()
            .GreaterThanOrEqual(0).WithMessage("Wiek nie może być ujemny.")
            .LessThanOrEqual(150).WithMessage("Wiek jest nierealistyczny.");
    }
}

public class PasswordStrengthValidator : ValueValidator<string>
{
    public PasswordStrengthValidator() : base("Password")
    {
        Check()
            .NotEmpty().WithMessage("Hasło jest wymagane.")
            .MinLength(8).WithMessage("Musi mieć co najmniej 8 znaków.")
            .Matches(@"[A-Z]").WithMessage("Musi zawierać wielką literę.")
                .WithSeverity(Severity.AtOwnRisk)
            .Matches(@"\d").WithMessage("Musi zawierać cyfrę.")
                .WithSeverity(Severity.AtOwnRisk);
    }
}
```

### Fabryka inline

Do szybkiej, doraźnej walidacji bez dedykowanej klasy użyj `ValueValidator.Create<T>()`:

```csharp
var phoneValidator = ValueValidator.Create<string>(
    configure: b => b
        .NotEmpty().WithMessage("Numer telefonu jest wymagany.")
        .PhoneNumber().WithMessage("Nieprawidłowy format telefonu."),
    propertyName: "Phone");

var result = phoneValidator.Validate("+48 123 456 789");
// result.IsValid == true
```

```csharp
var percentValidator = ValueValidator.Create<decimal>(
    configure: b => b
        .InclusiveBetween(0m, 100m).WithMessage("Musi być wartością procentową (0–100)."),
    propertyName: "Discount");

var result = percentValidator.Validate(105m);
// result.IsValid == false
```

Gdy pominiesz `propertyName`, domyślnie przyjmowana jest nazwa typu (np. `"String"`, `"Int32"`):

```csharp
var notEmpty = ValueValidator.Create<string>(b => b.NotEmpty());
var result = notEmpty.Validate("");
// result.Errors[0].PropertyName == "String"
```

### Reguły i modyfikatory w trybie samodzielnym

Samodzielne walidatory obsługują te same reguły i modyfikatory co walidatory obiektowe:

**Reguły ogólne** (dowolny typ): `.NotNull()`, `.Null()`, `.NotEmpty()`, `.Empty()`, `.Equal(value)`, `.NotEqual(value)`, `.Must(predicate, msg)`

**Reguły tekstowe:** `.MinLength(n)`, `.MaxLength(n)`, `.Length(min, max)`, `.Matches(pattern)`, `.EmailAddress()`, `.PhoneNumber()`

**Reguły porównawcze** (`IComparable<T>`): `.GreaterThan(n)`, `.GreaterThanOrEqual(n)`, `.LessThan(n)`, `.LessThanOrEqual(n)`, `.InclusiveBetween(a, b)`

**Modyfikatory:** `.WithMessage("...")`, `.WithErrorCode("...")`, `.WithSeverity(Severity.X)`, `.WithLayerSeverity("api", Severity.X)`, `.Cascade(CascadeMode.StopOnFirstFailure)`

### Obsługa warstw w trybie samodzielnym

Samodzielne walidatory w pełni obsługują wielowarstwowe nadpisywanie istotności:

```csharp
public class TaxIdValidator : ValueValidator<string>
{
    public TaxIdValidator() : base("TaxId")
    {
        Check()
            .NotEmpty().WithMessage("NIP jest wymagany.")
            .WithSeverity(Severity.Forbidden)
            .WithLayerSeverity("api", Severity.Forbidden)
            .WithLayerSeverity("dto", Severity.AtOwnRisk)
            .WithLayerSeverity("entity", Severity.NotRecommended);
    }
}

var validator = new TaxIdValidator();

// Domyślna istotność: Forbidden
var defaultResult = validator.Validate("");
// defaultResult.Errors[0].Severity == Severity.Forbidden

// Warstwa API: nadal Forbidden
var apiResult = validator.Validate("", ValidationContext.ForLayer("api"));
// apiResult.Errors[0].Severity == Severity.Forbidden

// Warstwa encji: tylko zalecenie
var entityResult = validator.Validate("", ValidationContext.ForLayer("entity"));
// entityResult.Errors[0].Severity == Severity.NotRecommended
```

### Warunki oparte na wartości

W walidatorach obiektowych `When()` / `Unless()` otrzymują instancję obiektu nadrzędnego. W trybie samodzielnym otrzymują samą wartość:

```csharp
var validator = ValueValidator.Create<string>(
    configure: b => b
        // Waliduj długość tylko gdy wartość nie jest null
        .When(value => value is not null)
        .MinLength(3).WithMessage("Za krótkie — musi mieć co najmniej 3 znaki.")
        .MaxLength(50).WithMessage("Za długie — co najwyżej 50 znaków."),
    propertyName: "Code");

validator.Validate(null!).IsValid;   // true  — warunek pominięty
validator.Validate("ab").IsValid;    // false — MinLength nie przeszedł
validator.Validate("abc").IsValid;   // true
```

```csharp
var validator = ValueValidator.Create<string>(
    configure: b => b
        // Pomiń walidację gdy wartość jest null (pole opcjonalne)
        .Unless(value => value is null)
        .EmailAddress().WithMessage("Nieprawidłowy adres email."),
    propertyName: "SecondaryEmail");

validator.Validate(null!).IsValid;          // true  — pominięto
validator.Validate("bad").IsValid;          // false — sprawdzono
validator.Validate("a@b.com").IsValid;      // true  — prawidłowy
```

### Wielokrotne łańcuchy sprawdzeń

Wywołaj `Check()` wielokrotnie wewnątrz walidatora opartego na klasie, aby utworzyć niezależne łańcuchy reguł. W połączeniu z `CascadeMode.StopOnFirstFailure` na walidatorze możesz precyzyjnie kontrolować przepływ:

```csharp
public class StrictCodeValidator : ValueValidator<string>
{
    public StrictCodeValidator() : base("Code")
    {
        CascadeMode = CascadeMode.StopOnFirstFailure;

        Check().NotNull().WithMessage("Kod jest wymagany.");
        Check().MinLength(5).WithMessage("Kod jest za krótki.");
        Check().Matches(@"^[A-Z0-9]+$").WithMessage("Kod musi składać się z wielkich liter i cyfr.");
    }
}

var validator = new StrictCodeValidator();
var result = validator.Validate(null!);
// result.Errors.Count == 1  (zatrzymano po "Kod jest wymagany.")
```

### Interfejs niegeneryczny

Podobnie jak `IValidator`, system samodzielny udostępnia niegeneryczny interfejs `IValueValidator` dla DI i scenariuszy w czasie wykonywania:

```csharp
IValueValidator validator = new EmailValidator();

Console.WriteLine(validator.ValidatedType);        // System.String
var result = validator.Validate("test@test.com");   // działa z object?
```

### Kiedy używać walidacji samodzielnej a obiektowej

| Scenariusz | Podejście |
|----------|----------|
| Walidacja pojedynczego pola formularza | **Samodzielna** (`ValueValidator<string>`) |
| Walidacja modelu żądania API | **Obiektowa** (`AbstractValidator<T>`) |
| Wielokrotnie używalne sprawdzenie "czy to prawidłowy email?" | **Samodzielna** (`EmailValidator`) |
| Reguła międzywłaściwościowa (koniec > data rozpoczęcia) | **Obiektowa** (`.Must((obj, val) => ...)`) |
| Blazor `@onblur` na pojedynczym `<input>` | **Samodzielna** lub `ValidateProperty` |
| Walidacja pojedynczego pola w TypeScript | **Samodzielna** (ten sam model mentalny) |
| Pełny zapis / wysłanie modelu | **Obiektowa** (`AbstractValidator<T>`) |
| Współdzielona logika walidacji między modelami | **Samodzielna** (komponuj przez `.Must()` lub `Combine`) |

### Kompozycja samodzielnych z obiektowymi walidatorami

Ponowne użycie samodzielnego walidatora wewnątrz walidatora obiektowego:

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
                  "Musi być prawidłowym adresem email.");

        RuleFor(x => x.Phone)
            .Must(value => phoneValidator.Validate(value).IsValid,
                  "Musi być prawidłowym numerem telefonu.");
    }
}
```

Lub jawne scalanie wyników:

```csharp
var emailResult = emailValidator.Validate(formData.Email);
var phoneResult = phoneValidator.Validate(formData.Phone);
var combined = ValidationResult.Combine(emailResult, phoneResult);

if (combined.HasForbidden) { /* ... */ }
```

---

## Integracja z Blazor

### Komponenty

**Podsumowanie walidacji** -- wyświetla wszystkie niepowodzenia pogrupowane według istotności:

```razor
@using Common.Validation.Blazor

<CvValidationSummary Result="@_validationResult" />
```

**Komunikaty per pole** -- wyświetla niepowodzenia dla konkretnej właściwości:

```razor
<InputText @bind-Value="_model.Email" class="form-control" />
<CvValidationMessage Result="@_result" PropertyName="Email" />
```

Oba komponenty używają klas CSS do stylowania opartego na istotności:
- `.cv-forbidden` -- czerwony
- `.cv-at-own-risk` -- bursztynowy
- `.cv-not-recommended` -- szary

### Integracja z EditContext

Podłączenie do wbudowanej walidacji formularzy Blazor:

```csharp
@using Common.Validation.Blazor

<EditForm EditContext="@_editContext" OnSubmit="HandleSubmit">
    <InputText @bind-Value="_model.Name" />
    <ValidationMessage For="@(() => _model.Name)" />

    <button type="submit">Zapisz</button>
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

## Klient TypeScript

Pakiet TypeScript konsumuje te same definicje JSON co backend C#:

```typescript
import { Validator } from 'common-validation';
import type { ValidationDefinition } from 'common-validation';

// Wczytaj tę samą definicję JSON używaną przez backend
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

Walidacja uwzględniająca warstwy działa tak samo:

```typescript
// Waliduj z konkretną warstwą
const result = validator.validate(formData, 'api');
```

Rejestracja niestandardowych walidatorów po stronie klienta:

```typescript
import { Validator } from 'common-validation';

const validator = new Validator(definition, {
  iban: () => (value) =>
    typeof value === 'string' && value.length >= 15 && value.length <= 34
});
```

---

## Architektura

```
common.validation/
  src/
    Common.Validation/                   # Główny pakiet NuGet
      Core/                              #   IValidator, AbstractValidator, Severity,
                                         #   ValidationResult, CascadeMode, ValidationContext
                                         #   IValueValidator, ValueValidator (samodzielna)
      Rules/                             #   IRuleBuilder, IValidationRule, PropertyRule
                                         #   IValueRuleBuilder, IValueValidationRule, ValueRule (samodzielna)
      Extensions/                        #   Fluent API (NotEmpty, MaxLength, WithSeverity, itp.)
                                         #   Rozszerzenia samodzielne (warianty *ValueRule*)
      Layers/                            #   ValidationLayerAttribute
      Json/                              #   JsonValidator, modele definicji, loader
        Registry/                        #   IValidatorTypeRegistry, wbudowane kontrole
      DependencyInjection/               #   AddCommonValidation, IValidatorFactory
    Common.Validation.Blazor/            # Pakiet NuGet Blazor (Razor Class Library)
                                         #   CvValidationSummary, CvValidationMessage,
                                         #   rozszerzenia EditContext, izolacja CSS
  client/
    common-validation/                   # Pakiet npm TypeScript
      src/                              #   Validator, typy, wbudowane reguły
      schema/                           #   JSON Schema dla autouzupełniania w IDE
  tests/
    Common.Validation.Tests/             # Testy xUnit (213 testów)
  demo/
    Common.Validation.Demo/              # Demo konsolowe (fluent, warstwy, JSON, DI)
    Common.Validation.Demo.Blazor/       # Demo interaktywne Blazor
```

---

## Przypadki użycia

### Walidacja danych wejściowych REST API

Waliduj przychodzące żądania w kontrolerze lub middleware, odrzucając niepowodzenia `Forbidden` i przekazując `AtOwnRisk` / `NotRecommended` jako nagłówki odpowiedzi lub metadane.

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

### Konfigurowalna walidacja wielodostępna (multi-tenant)

Wczytuj reguły walidacji z bazy danych lub API konfiguracyjnego dla każdego tenanta, używając definicji JSON:

```csharp
builder.Services.AddCommonValidation(options =>
{
    options.JsonDefinitionPaths.Add($"Validations/{tenantId}/");
});
```

### Kreator formularzy z progresywną rygorystycznością

W formularzu wielokrokowym wcześniejsze kroki używają łagodnej istotności; ostatni krok wymusza wszystko:

```csharp
// Krok 1: pokaż tylko wskazówki
var step1Context = ValidationContext.ForLayer("draft");

// Krok 2: ostrzeż o ryzykach
var step2Context = ValidationContext.ForLayer("review");

// Końcowe wysłanie: blokuj przy forbidden
var submitContext = ValidationContext.ForLayer("api");
```

### Współdzielona walidacja klient-serwer

Zdefiniuj reguły w JSON, serwuj je z API, waliduj po obu stronach:

```
Backend:  JsonValidator<T>(definition)          --> ValidationResult
Frontend: new Validator(definition).validate(t) --> ValidationResult
```

Oba generują te same niepowodzenia dla tych samych danych wejściowych, zapewniając zgodność komunikatów błędów w UI z egzekwowaniem po stronie serwera.

### PATCH API z walidacją per pole opartą na JSON

Przy obsłudze częściowych aktualizacji (PATCH) waliduj tylko pola, które się zmieniły, używając reguł z definicji JSON:

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

Pozwala to uniknąć walidacji nietkniętych pól i zachowuje spójność reguł ze współdzieloną definicją JSON.

### Wielokrotnie używalne walidatory pól w wielu modelach

Zdefiniuj walidację raz dla koncepcji danych, używaj jej ponownie w każdym modelu, który ją zawiera:

```csharp
// Zdefiniuj raz
public class EmailValidator : ValueValidator<string>
{
    public EmailValidator() : base("Email")
    {
        Check()
            .NotEmpty().WithMessage("Email jest wymagany.")
            .EmailAddress().WithMessage("Musi być prawidłowym adresem email.")
            .MaxLength(255);
    }
}

public class PhoneValidator : ValueValidator<string>
{
    public PhoneValidator() : base("Phone")
    {
        Check()
            .NotEmpty().WithMessage("Telefon jest wymagany.")
            .PhoneNumber().WithMessage("Musi być prawidłowym numerem telefonu.");
    }
}

// Używaj ponownie w walidatorach obiektowych
var email = new EmailValidator();
var phone = new PhoneValidator();

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Email)
            .Must(v => email.Validate(v).IsValid, "Nieprawidłowy email.");

        RuleFor(x => x.Phone)
            .Must(v => phone.Validate(v).IsValid, "Nieprawidłowy telefon.");
    }
}

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.WorkEmail)
            .Must(v => email.Validate(v).IsValid, "Nieprawidłowy email służbowy.");
    }
}
```

### Niezmienniki encji domenowych

Waliduj niezmienniki wewnątrz samych encji domenowych, używając niegenerycznego interfejsu `IValidator` do polimorficznego dispatchu:

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

### Złożona walidacja w ramach agregatów

Waliduj cały korzeń agregatu, scalając wyniki z walidatorów podrzędnych:

```csharp
var orderResult = _orderValidator.Validate(order);
var itemResults = order.Items.Select(item => _itemValidator.Validate(item));

var combined = ValidationResult.Combine(
    new[] { orderResult }.Concat(itemResults).ToArray());

if (combined.HasForbidden) { /* ... */ }
```

---

## Plan rozwoju

Projekt jest aktywnie rozwijany. Planowane kierunki to:

- **Walidacja asynchroniczna** -- `ValidateAsync` dla reguł wymagających operacji I/O (np. sprawdzanie unikalności w bazie danych). Interfejs `IValidator<T>` jest zaprojektowany, aby to wspierać.
- **Lokalizacja** -- szablony komunikatów z placeholderami i integracja z plikami zasobów dla obsługi wielu języków.
- **Generatory źródłowe** -- wykrywanie walidatorów w czasie kompilacji i serializacja JSON kompatybilna z AOT, eliminująca narzut refleksji.
- **Profile walidacji** -- nazwane zestawy reguł w ramach jednego walidatora, wybieralne w czasie walidacji (np. profile "create" vs "update").
- **Integracja z OpenAPI** -- automatyczne generowanie metadanych `x-validation` w specyfikacjach Swagger/OpenAPI z definicji JSON.
- **Adaptery React / Vue** -- obudowy specyficzne dla frameworków wokół klienta TypeScript dla bezproblemowej integracji z formularzami.
- **Kompozycja reguł** -- `Include()` i `InheritRulesFrom()` do komponowania walidatorów z wielokrotnie używalnych fragmentów. Automatyczne mostkowanie `IValueValidator<T>` do reguł `AbstractValidator<T>` za pomocą dedykowanej metody rozszerzającej.
- **Analizatory diagnostyczne** -- analizatory Roslyn do wykrywania typowych błędów w czasie kompilacji (np. zapomnienie `.WithMessage()` po regule).

---

## Licencja

Projekt jest licencjonowany na zasadach **GNU Affero General Public License v3.0** (AGPL-3.0-or-later).
Pełny tekst licencji znajduje się w pliku [LICENSE.txt](LICENSE.txt).
