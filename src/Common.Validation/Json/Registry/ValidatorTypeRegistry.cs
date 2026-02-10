using System.Text.Json;
using System.Text.RegularExpressions;

namespace Common.Validation.Json.Registry;

/// <summary>
/// Default implementation of <see cref="IValidatorTypeRegistry"/>
/// with all built-in validators pre-registered.
/// </summary>
public class ValidatorTypeRegistry : IValidatorTypeRegistry
{
    private readonly Dictionary<string, Func<JsonElement?, IPropertyCheck>> _factories = new(StringComparer.OrdinalIgnoreCase);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(factory);
        _factories[name] = factory;
    }

    /// <inheritdoc />
    public IPropertyCheck Resolve(string name, JsonElement? parameters)
    {
        if (!_factories.TryGetValue(name, out var factory))
            throw new InvalidOperationException($"Validator type '{name}' is not registered.");

        return factory(parameters);
    }

    /// <inheritdoc />
    public bool IsRegistered(string name) => _factories.ContainsKey(name);

    private void RegisterBuiltIns()
    {
        Register("notNull", _ => new DelegateCheck(v => v is not null));
        Register("null", _ => new DelegateCheck(v => v is null));

        Register("notEmpty", _ => new DelegateCheck(v => v switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            System.Collections.ICollection { Count: 0 } => false,
            _ => true
        }));

        Register("empty", _ => new DelegateCheck(v => v switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            System.Collections.ICollection { Count: 0 } => true,
            _ => false
        }));

        Register("maxLength", p =>
        {
            var max = GetRequiredInt(p, "max");
            return new DelegateCheck(v => v is null || (v is string s && s.Length <= max));
        });

        Register("minLength", p =>
        {
            var min = GetRequiredInt(p, "min");
            return new DelegateCheck(v => v is string s && s.Length >= min);
        });

        Register("length", p =>
        {
            var min = GetRequiredInt(p, "min");
            var max = GetRequiredInt(p, "max");
            return new DelegateCheck(v => v is string s && s.Length >= min && s.Length <= max);
        });

        Register("email", _ => new DelegateCheck(v =>
            v is string s && Regex.IsMatch(s, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)));

        Register("phone", _ => new DelegateCheck(v =>
            v is string s && Regex.IsMatch(s, @"^\+?[\d\s\-\(\)]{7,20}$")));

        Register("matches", p =>
        {
            var pattern = GetRequiredString(p, "pattern");
            var regex = new Regex(pattern, RegexOptions.Compiled);
            return new DelegateCheck(v => v is string s && regex.IsMatch(s));
        });

        Register("equal", p =>
        {
            var expected = GetRequiredString(p, "value");
            return new DelegateCheck(v => string.Equals(v?.ToString(), expected, StringComparison.Ordinal));
        });

        Register("notEqual", p =>
        {
            var expected = GetRequiredString(p, "value");
            return new DelegateCheck(v => !string.Equals(v?.ToString(), expected, StringComparison.Ordinal));
        });

        Register("greaterThan", p =>
        {
            var threshold = GetRequiredDouble(p, "value");
            return new DelegateCheck(v => v is IComparable c && Convert.ToDouble(c) > threshold);
        });

        Register("greaterThanOrEqual", p =>
        {
            var threshold = GetRequiredDouble(p, "value");
            return new DelegateCheck(v => v is IComparable c && Convert.ToDouble(c) >= threshold);
        });

        Register("lessThan", p =>
        {
            var threshold = GetRequiredDouble(p, "value");
            return new DelegateCheck(v => v is IComparable c && Convert.ToDouble(c) < threshold);
        });

        Register("lessThanOrEqual", p =>
        {
            var threshold = GetRequiredDouble(p, "value");
            return new DelegateCheck(v => v is IComparable c && Convert.ToDouble(c) <= threshold);
        });

        Register("inclusiveBetween", p =>
        {
            var from = GetRequiredDouble(p, "from");
            var to = GetRequiredDouble(p, "to");
            return new DelegateCheck(v =>
            {
                if (v is not IComparable) return false;
                var d = Convert.ToDouble(v);
                return d >= from && d <= to;
            });
        });
    }

    #region Parameter helpers

    private static int GetRequiredInt(JsonElement? parameters, string name)
    {
        if (parameters is null || parameters.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException($"Parameter '{name}' is required.");

        if (parameters.Value.TryGetProperty(name, out var prop) && prop.TryGetInt32(out var value))
            return value;

        throw new InvalidOperationException($"Parameter '{name}' must be an integer.");
    }

    private static string GetRequiredString(JsonElement? parameters, string name)
    {
        if (parameters is null || parameters.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException($"Parameter '{name}' is required.");

        if (parameters.Value.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString()!;

        throw new InvalidOperationException($"Parameter '{name}' must be a string.");
    }

    private static double GetRequiredDouble(JsonElement? parameters, string name)
    {
        if (parameters is null || parameters.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException($"Parameter '{name}' is required.");

        if (parameters.Value.TryGetProperty(name, out var prop) && prop.TryGetDouble(out var value))
            return value;

        throw new InvalidOperationException($"Parameter '{name}' must be a number.");
    }

    #endregion

    #region Inner types

    private sealed class DelegateCheck : IPropertyCheck
    {
        private readonly Func<object?, bool> _predicate;

        public DelegateCheck(Func<object?, bool> predicate) => _predicate = predicate;

        public bool IsValid(object? value) => _predicate(value);
    }

    #endregion
}
