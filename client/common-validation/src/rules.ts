import type { CheckFactory, PropertyCheck } from './types';

/**
 * Registry of built-in validator type implementations.
 * Maps validator names to factory functions that create check predicates.
 */
const builtInRules: Record<string, CheckFactory> = {
  notNull: (): PropertyCheck => (value) => value !== null && value !== undefined,

  null: (): PropertyCheck => (value) => value === null || value === undefined,

  notEmpty: (): PropertyCheck => (value) => {
    if (value === null || value === undefined) return false;
    if (typeof value === 'string') return value.trim().length > 0;
    if (Array.isArray(value)) return value.length > 0;
    return true;
  },

  empty: (): PropertyCheck => (value) => {
    if (value === null || value === undefined) return true;
    if (typeof value === 'string') return value.trim().length === 0;
    if (Array.isArray(value)) return value.length === 0;
    return false;
  },

  maxLength: (params): PropertyCheck => {
    const max = requireNumber(params, 'max');
    return (value) => value === null || value === undefined || (typeof value === 'string' && value.length <= max);
  },

  minLength: (params): PropertyCheck => {
    const min = requireNumber(params, 'min');
    return (value) => typeof value === 'string' && value.length >= min;
  },

  length: (params): PropertyCheck => {
    const min = requireNumber(params, 'min');
    const max = requireNumber(params, 'max');
    return (value) => typeof value === 'string' && value.length >= min && value.length <= max;
  },

  email: (): PropertyCheck => {
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/i;
    return (value) => typeof value === 'string' && emailRegex.test(value);
  },

  phone: (): PropertyCheck => {
    const phoneRegex = /^\+?[\d\s\-()]{7,20}$/;
    return (value) => typeof value === 'string' && phoneRegex.test(value);
  },

  matches: (params): PropertyCheck => {
    const pattern = requireString(params, 'pattern');
    const regex = new RegExp(pattern);
    return (value) => typeof value === 'string' && regex.test(value);
  },

  equal: (params): PropertyCheck => {
    const expected = requireString(params, 'value');
    return (value) => String(value) === expected;
  },

  notEqual: (params): PropertyCheck => {
    const expected = requireString(params, 'value');
    return (value) => String(value) !== expected;
  },

  greaterThan: (params): PropertyCheck => {
    const threshold = requireNumber(params, 'value');
    return (value) => typeof value === 'number' && value > threshold;
  },

  greaterThanOrEqual: (params): PropertyCheck => {
    const threshold = requireNumber(params, 'value');
    return (value) => typeof value === 'number' && value >= threshold;
  },

  lessThan: (params): PropertyCheck => {
    const threshold = requireNumber(params, 'value');
    return (value) => typeof value === 'number' && value < threshold;
  },

  lessThanOrEqual: (params): PropertyCheck => {
    const threshold = requireNumber(params, 'value');
    return (value) => typeof value === 'number' && value <= threshold;
  },

  inclusiveBetween: (params): PropertyCheck => {
    const from = requireNumber(params, 'from');
    const to = requireNumber(params, 'to');
    return (value) => typeof value === 'number' && value >= from && value <= to;
  },
};

function requireNumber(params: Record<string, unknown> | undefined, name: string): number {
  if (!params || typeof params[name] !== 'number') {
    throw new Error(`Parameter '${name}' must be a number.`);
  }
  return params[name] as number;
}

function requireString(params: Record<string, unknown> | undefined, name: string): string {
  if (!params || typeof params[name] !== 'string') {
    throw new Error(`Parameter '${name}' must be a string.`);
  }
  return params[name] as string;
}

export { builtInRules };
