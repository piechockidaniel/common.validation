import { builtInRules } from './rules';
import type {
  CheckFactory,
  PropertyCheck,
  Severity,
  ValidationDefinition,
  ValidationFailure,
  ValidationResult,
} from './types';

const DEFAULT_SEVERITY: Severity = 'forbidden';

interface CompiledCheck {
  check: PropertyCheck;
  message: string;
  errorCode?: string;
  defaultSeverity: Severity;
  layerSeverities?: Record<string, Severity>;
}

interface CompiledPropertyRule {
  propertyName: string;
  checks: CompiledCheck[];
}

/**
 * Client-side validator that applies rules from a JSON ValidationDefinition.
 * Mirrors the C# JsonValidator<T> implementation.
 */
export class Validator<T extends Record<string, unknown> = Record<string, unknown>> {
  private readonly compiledRules: CompiledPropertyRule[];
  private readonly customRules: Record<string, CheckFactory>;

  /**
   * Creates a new Validator from a validation definition.
   * @param definition The validation definition loaded from JSON.
   * @param customRules Optional custom rule factories to extend or override built-in rules.
   */
  constructor(
    definition: ValidationDefinition,
    customRules?: Record<string, CheckFactory>,
  ) {
    this.customRules = customRules ?? {};
    this.compiledRules = this.compile(definition);
  }

  /**
   * Validates an instance and returns a ValidationResult.
   * @param instance The object to validate.
   * @param layer Optional validation layer for layer-specific severity resolution.
   */
  validate(instance: T, layer?: string): ValidationResult {
    const errors: ValidationFailure[] = [];

    for (const rule of this.compiledRules) {
      const value = instance[rule.propertyName];

      for (const check of rule.checks) {
        if (!check.check(value)) {
          const severity = resolveSeverity(check, layer);
          errors.push({
            propertyName: rule.propertyName,
            errorMessage: check.message,
            errorCode: check.errorCode,
            severity,
            attemptedValue: value,
          });
        }
      }
    }

    return {
      isValid: errors.length === 0,
      errors,
      hasForbidden: errors.some((e) => e.severity === 'forbidden'),
      hasAtOwnRisk: errors.some((e) => e.severity === 'atOwnRisk'),
      hasNotRecommended: errors.some((e) => e.severity === 'notRecommended'),
    };
  }

  private compile(definition: ValidationDefinition): CompiledPropertyRule[] {
    const rules: CompiledPropertyRule[] = [];

    for (const [propertyName, propertyDef] of Object.entries(definition.properties)) {
      const checks: CompiledCheck[] = [];

      for (const ruleDef of propertyDef.rules) {
        const factory = this.customRules[ruleDef.validator] ?? builtInRules[ruleDef.validator];
        if (!factory) {
          throw new Error(`Validator type '${ruleDef.validator}' is not registered.`);
        }

        const check = factory(ruleDef.params);

        checks.push({
          check,
          message: ruleDef.message,
          errorCode: ruleDef.errorCode,
          defaultSeverity: ruleDef.severity ?? DEFAULT_SEVERITY,
          layerSeverities: ruleDef.layers,
        });
      }

      rules.push({ propertyName, checks });
    }

    return rules;
  }
}

function resolveSeverity(check: CompiledCheck, layer?: string): Severity {
  if (layer && check.layerSeverities && check.layerSeverities[layer]) {
    return check.layerSeverities[layer];
  }
  return check.defaultSeverity;
}
