/**
 * Severity levels matching the C# Common.Validation.Core.Severity enum.
 */
export type Severity = 'notRecommended' | 'atOwnRisk' | 'forbidden';

/**
 * Root validation definition for a single type.
 * Mirrors the C# Common.Validation.Json.Models.ValidationDefinition class.
 */
export interface ValidationDefinition {
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
export interface PropertyDefinition {
  /** List of validation rules for this property. */
  rules: RuleDefinition[];
}

/**
 * A single validation rule within a property definition.
 * Mirrors the C# Common.Validation.Json.Models.RuleDefinition class.
 */
export interface RuleDefinition {
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
export interface ValidationFailure {
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
export interface ValidationResult {
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
export type PropertyCheck = (value: unknown) => boolean;

/**
 * Factory function that creates a PropertyCheck from optional JSON parameters.
 */
export type CheckFactory = (params?: Record<string, unknown>) => PropertyCheck;
