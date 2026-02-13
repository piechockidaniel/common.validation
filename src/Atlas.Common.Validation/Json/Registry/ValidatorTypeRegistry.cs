using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Atlas.Common.Validation.Json.Registry;

/// <summary>
/// Default implementation of <see cref="IValidatorTypeRegistry"/>
/// with all built-in validators pre-registered.
/// </summary>
public class ValidatorTypeRegistry : IValidatorTypeRegistry
{
    private readonly Dictionary<string, Func<JsonElement?, IPropertyCheck>> _factories = new(comparer: StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new <see cref="ValidatorTypeRegistry"/> with built-in validators registered.
    /// </summary>
    public ValidatorTypeRegistry()
    {
        RegisterBuiltIns();
    }

    /// <inheritdoc />
    public void Register(string name, Func<JsonElement?, IPropertyCheck> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: name);
        ArgumentNullException.ThrowIfNull(argument: factory);
        _factories[key: name] = factory;
    }

    /// <inheritdoc />
    public IPropertyCheck Resolve(string name, JsonElement? parameters)
    {
        if (!_factories.TryGetValue(key: name, value: out var factory))
            throw new InvalidOperationException(message: $"Validator type '{name}' is not registered.");

        return factory(arg: parameters);
    }

    /// <inheritdoc />
    public bool IsRegistered(string name) => _factories.ContainsKey(key: name);

    private void RegisterBuiltIns()
    {
        Register(name: "notNull", factory: _ => new DelegateCheck(predicate: v => v is not null));
        Register(name: "null", factory: _ => new DelegateCheck(predicate: v => v is null));

        Register(name: "notEmpty", factory: _ => new DelegateCheck(predicate: v => v switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(value: s),
            System.Collections.ICollection { Count: 0 } => false,
            _ => true,
        }));

        Register(name: "empty", factory: _ => new DelegateCheck(predicate: v => v switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(value: s),
            System.Collections.ICollection { Count: 0 } => true,
            _ => false,
        }));

        Register(name: "maxLength", factory: p =>
        {
            var max = GetRequiredInt(parameters: p, name: "max");
            return new DelegateCheck(predicate: v => v is null || (v is string s && s.Length <= max));
        });

        Register(name: "minLength", factory: p =>
        {
            var min = GetRequiredInt(parameters: p, name: "min");
            return new DelegateCheck(predicate: v => v is string s && s.Length >= min);
        });

        Register(name: "length", factory: p =>
        {
            var min = GetRequiredInt(parameters: p, name: "min");
            var max = GetRequiredInt(parameters: p, name: "max");
            return new DelegateCheck(predicate: v => v is string s && s.Length >= min && s.Length <= max);
        });

        Register(name: "email", factory: _ => new DelegateCheck(predicate: v =>
            v is string s && Regex.IsMatch(input: s, pattern: @"^[^@\s]+@[^@\s]+\.[^@\s]+$", options: RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500))));

        Register(name: "phone", factory: _ => new DelegateCheck(predicate: v =>
            v is string s && Regex.IsMatch(input: s, pattern: @"^\+?[\d\s\-\(\)]{7,20}$", options: RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500))));

        Register(name: "matches", factory: p =>
        {
            var pattern = GetRequiredString(parameters: p, name: "pattern");
            var regex = new Regex(pattern: pattern, options: RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));
            return new DelegateCheck(predicate: v => v is string s && regex.IsMatch(input: s));
        });

        Register(name: "equal", factory: p =>
        {
            var expected = GetRequiredString(parameters: p, name: "value");
            return new DelegateCheck(predicate: v => string.Equals(a: v?.ToString(), b: expected, comparisonType: StringComparison.Ordinal));
        });

        Register(name: "notEqual", factory: p =>
        {
            var expected = GetRequiredString(parameters: p, name: "value");
            return new DelegateCheck(predicate: v => !string.Equals(a: v?.ToString(), b: expected, comparisonType: StringComparison.Ordinal));
        });

        Register(name: "greaterThan", factory: p =>
        {
            var threshold = GetRequiredDouble(parameters: p, name: "value");
            return new DelegateCheck(predicate: v => v is IComparable c && Convert.ToDouble(value: c, provider: NumberFormatInfo.InvariantInfo) > threshold);
        });

        Register(name: "greaterThanOrEqual", factory: p =>
        {
            var threshold = GetRequiredDouble(parameters: p, name: "value");
            return new DelegateCheck(predicate: v => v is IComparable c && Convert.ToDouble(value: c, provider: NumberFormatInfo.InvariantInfo) >= threshold);
        });

        Register(name: "lessThan", factory: p =>
        {
            var threshold = GetRequiredDouble(parameters: p, name: "value");
            return new DelegateCheck(predicate: v => v is IComparable c && Convert.ToDouble(value: c, provider: NumberFormatInfo.InvariantInfo) < threshold);
        });

        Register(name: "lessThanOrEqual", factory: p =>
        {
            var threshold = GetRequiredDouble(parameters: p, name: "value");
            return new DelegateCheck(predicate: v => v is IComparable c && Convert.ToDouble(value: c, provider: NumberFormatInfo.InvariantInfo) <= threshold);
        });

        Register(name: "inclusiveBetween", factory: p =>
        {
            var from = GetRequiredDouble(parameters: p, name: "from");
            var to = GetRequiredDouble(parameters: p, name: "to");
            return new DelegateCheck(predicate: v =>
            {
                if (v is not IComparable) return false;
                var d = Convert.ToDouble(value: v, provider: NumberFormatInfo.InvariantInfo);
                return d >= from && d <= to;
            });
        });
    }

    #region Parameter helpers

    private static int GetRequiredInt(JsonElement? parameters, string name)
    {
        if (parameters is null || parameters.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException(message: $"Parameter '{name}' is required.");

        if (parameters.Value.TryGetProperty(propertyName: name, value: out var prop) && prop.TryGetInt32(value: out var value))
            return value;

        throw new InvalidOperationException(message: $"Parameter '{name}' must be an integer.");
    }

    private static string GetRequiredString(JsonElement? parameters, string name)
    {
        if (parameters is null || parameters.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException(message: $"Parameter '{name}' is required.");

        if (parameters.Value.TryGetProperty(propertyName: name, value: out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString()!;

        throw new InvalidOperationException(message: $"Parameter '{name}' must be a string.");
    }

    private static double GetRequiredDouble(JsonElement? parameters, string name)
    {
        if (parameters is null || parameters.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException(message: $"Parameter '{name}' is required.");

        if (parameters.Value.TryGetProperty(propertyName: name, value: out var prop) && prop.TryGetDouble(value: out var value))
            return value;

        throw new InvalidOperationException(message: $"Parameter '{name}' must be a number.");
    }

    #endregion

    #region Inner types

    private sealed class DelegateCheck(Func<object?, bool> predicate) : IPropertyCheck
    {
        public bool IsValid(object? value) => predicate(arg: value);
    }

    #endregion
}
