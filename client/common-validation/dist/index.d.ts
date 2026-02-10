/**
 * Severity levels matching the C# Common.Validation.Core.Severity enum.
 */
type Severity = 'notRecommended' | 'atOwnRisk' | 'forbidden';
/**
 * Root validation definition for a single type.
 * Mirrors the C# Common.Validation.Json.Models.ValidationDefinition class.
 */
interface ValidationDefinition {
    /** Optional JSON Schema reference for editor support. */
    $schema?: string;
    /** The type name this definition applies to. */
    type: string;
    /** Property validation definitions keyed by property name. */
    properties: Record<string, PropertyDefinition>;
}
/**
 * Validation rules for a single property.
 * Mirrors the C# Common.Validation.Json.Models.PropertyDefinition class.
 */
interface PropertyDefinition {
    /** List of validation rules for this property. */
    rules: RuleDefinition[];
}
/**
 * A single validation rule within a property definition.
 * Mirrors the C# Common.Validation.Json.Models.RuleDefinition class.
 */
interface RuleDefinition {
    /** The validator type name (e.g. "notEmpty", "maxLength", "email"). */
    validator: string;
    /** Optional parameters for the validator (e.g. { max: 100 }). */
    params?: Record<string, unknown>;
    /** The error message to display when validation fails. */
    message: string;
    /** Optional error code for programmatic handling. */
    errorCode?: string;
    /** Default severity level. */
    severity?: Severity;
    /** Layer-specific severity overrides. Keys are layer names, values are severity levels. */
    layers?: Record<string, Severity>;
}
/**
 * Represents a single validation failure.
 * Mirrors the C# Common.Validation.Core.ValidationFailure class.
 */
interface ValidationFailure {
    /** The name of the property that failed validation. */
    propertyName: string;
    /** The error message describing the failure. */
    errorMessage: string;
    /** Optional error code for programmatic handling. */
    errorCode?: string;
    /** The severity level of this failure. */
    severity: Severity;
    /** The value that was validated. */
    attemptedValue?: unknown;
}
/**
 * Represents the result of a validation operation.
 * Mirrors the C# Common.Validation.Core.ValidationResult class.
 */
interface ValidationResult {
    /** Whether the validation was successful (no errors). */
    isValid: boolean;
    /** The collection of validation failures. */
    errors: ValidationFailure[];
    /** Whether there are any 'forbidden' failures. */
    hasForbidden: boolean;
    /** Whether there are any 'atOwnRisk' failures. */
    hasAtOwnRisk: boolean;
    /** Whether there are any 'notRecommended' failures. */
    hasNotRecommended: boolean;
}
/**
 * A validation check function that takes a value and returns true if valid.
 */
type PropertyCheck = (value: unknown) => boolean;
/**
 * Factory function that creates a PropertyCheck from optional JSON parameters.
 */
type CheckFactory = (params?: Record<string, unknown>) => PropertyCheck;

/**
 * Client-side validator that applies rules from a JSON ValidationDefinition.
 * Mirrors the C# JsonValidator<T> implementation.
 */
declare class Validator<T extends Record<string, unknown> = Record<string, unknown>> {
    private readonly compiledRules;
    private readonly customRules;
    /**
     * Creates a new Validator from a validation definition.
     * @param definition The validation definition loaded from JSON.
     * @param customRules Optional custom rule factories to extend or override built-in rules.
     */
    constructor(definition: ValidationDefinition, customRules?: Record<string, CheckFactory>);
    /**
     * Validates an instance and returns a ValidationResult.
     * @param instance The object to validate.
     * @param layer Optional validation layer for layer-specific severity resolution.
     */
    validate(instance: T, layer?: string): ValidationResult;
    private compile;
}

/**
 * Registry of built-in validator type implementations.
 * Maps validator names to factory functions that create check predicates.
 */
declare const builtInRules: Record<string, CheckFactory>;

export { type CheckFactory, type PropertyCheck, type PropertyDefinition, type RuleDefinition, type Severity, type ValidationDefinition, type ValidationFailure, type ValidationResult, Validator, builtInRules };
