// src/rules.ts
var builtInRules = {
  notNull: () => (value) => value !== null && value !== void 0,
  null: () => (value) => value === null || value === void 0,
  notEmpty: () => (value) => {
    if (value === null || value === void 0) return false;
    if (typeof value === "string") return value.trim().length > 0;
    if (Array.isArray(value)) return value.length > 0;
    return true;
  },
  empty: () => (value) => {
    if (value === null || value === void 0) return true;
    if (typeof value === "string") return value.trim().length === 0;
    if (Array.isArray(value)) return value.length === 0;
    return false;
  },
  maxLength: (params) => {
    const max = requireNumber(params, "max");
    return (value) => value === null || value === void 0 || typeof value === "string" && value.length <= max;
  },
  minLength: (params) => {
    const min = requireNumber(params, "min");
    return (value) => typeof value === "string" && value.length >= min;
  },
  length: (params) => {
    const min = requireNumber(params, "min");
    const max = requireNumber(params, "max");
    return (value) => typeof value === "string" && value.length >= min && value.length <= max;
  },
  email: () => {
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/i;
    return (value) => typeof value === "string" && emailRegex.test(value);
  },
  phone: () => {
    const phoneRegex = /^\+?[\d\s\-()]{7,20}$/;
    return (value) => typeof value === "string" && phoneRegex.test(value);
  },
  matches: (params) => {
    const pattern = requireString(params, "pattern");
    const regex = new RegExp(pattern);
    return (value) => typeof value === "string" && regex.test(value);
  },
  equal: (params) => {
    const expected = requireString(params, "value");
    return (value) => String(value) === expected;
  },
  notEqual: (params) => {
    const expected = requireString(params, "value");
    return (value) => String(value) !== expected;
  },
  greaterThan: (params) => {
    const threshold = requireNumber(params, "value");
    return (value) => typeof value === "number" && value > threshold;
  },
  greaterThanOrEqual: (params) => {
    const threshold = requireNumber(params, "value");
    return (value) => typeof value === "number" && value >= threshold;
  },
  lessThan: (params) => {
    const threshold = requireNumber(params, "value");
    return (value) => typeof value === "number" && value < threshold;
  },
  lessThanOrEqual: (params) => {
    const threshold = requireNumber(params, "value");
    return (value) => typeof value === "number" && value <= threshold;
  },
  inclusiveBetween: (params) => {
    const from = requireNumber(params, "from");
    const to = requireNumber(params, "to");
    return (value) => typeof value === "number" && value >= from && value <= to;
  }
};
function requireNumber(params, name) {
  if (!params || typeof params[name] !== "number") {
    throw new Error(`Parameter '${name}' must be a number.`);
  }
  return params[name];
}
function requireString(params, name) {
  if (!params || typeof params[name] !== "string") {
    throw new Error(`Parameter '${name}' must be a string.`);
  }
  return params[name];
}

// src/validator.ts
var DEFAULT_SEVERITY = "forbidden";
var Validator = class {
  compiledRules;
  customRules;
  /**
   * Creates a new Validator from a validation definition.
   * @param definition The validation definition loaded from JSON.
   * @param customRules Optional custom rule factories to extend or override built-in rules.
   */
  constructor(definition, customRules) {
    this.customRules = customRules ?? {};
    this.compiledRules = this.compile(definition);
  }
  /**
   * Validates an instance and returns a ValidationResult.
   * @param instance The object to validate.
   * @param layer Optional validation layer for layer-specific severity resolution.
   */
  validate(instance, layer) {
    const errors = [];
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
            attemptedValue: value
          });
        }
      }
    }
    return {
      isValid: errors.length === 0,
      errors,
      hasForbidden: errors.some((e) => e.severity === "forbidden"),
      hasAtOwnRisk: errors.some((e) => e.severity === "atOwnRisk"),
      hasNotRecommended: errors.some((e) => e.severity === "notRecommended")
    };
  }
  compile(definition) {
    const rules = [];
    for (const [propertyName, propertyDef] of Object.entries(definition.properties)) {
      const checks = [];
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
          layerSeverities: ruleDef.layers
        });
      }
      rules.push({ propertyName, checks });
    }
    return rules;
  }
};
function resolveSeverity(check, layer) {
  if (layer && check.layerSeverities && check.layerSeverities[layer]) {
    return check.layerSeverities[layer];
  }
  return check.defaultSeverity;
}
export {
  Validator,
  builtInRules
};
