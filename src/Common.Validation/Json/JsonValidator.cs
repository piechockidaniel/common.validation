using System.Reflection;
using Common.Validation.Core;
using Common.Validation.Json.Models;
using Common.Validation.Json.Registry;
using Common.Validation.Layers;

namespace Common.Validation.Json;

/// <summary>
/// A validator that applies rules loaded from a JSON <see cref="ValidationDefinition"/>.
/// Uses reflection to access properties of <typeparamref name="T"/> by name.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
public class JsonValidator<T> : IValidator<T>
{
    private readonly ValidationDefinition _definition;
    private readonly IValidatorTypeRegistry _registry;
    private readonly List<CompiledPropertyRule> _compiledRules;
    private static readonly string? CachedLayer = typeof(T)
        .GetCustomAttribute<ValidationLayerAttribute>(inherit: true)?.Layer;

    /// <summary>
    /// Gets or sets the cascade mode for this validator.
    /// </summary>
    public CascadeMode CascadeMode { get; set; } = CascadeMode.Continue;

    /// <summary>
    /// Creates a new <see cref="JsonValidator{T}"/> from a definition and optional custom registry.
    /// </summary>
    /// <param name="definition">The validation definition loaded from JSON.</param>
    /// <param name="registry">Optional custom registry. If <c>null</c>, uses the default with built-in validators.</param>
    public JsonValidator(ValidationDefinition definition, IValidatorTypeRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _definition = definition;
        _registry = registry ?? new ValidatorTypeRegistry();
        _compiledRules = CompileRules();
    }

    #region IValidator<T>

    /// <inheritdoc />
    public ValidationResult Validate(T instance)
    {
        return Validate(instance, new ValidationContext(CachedLayer));
    }

    /// <inheritdoc />
    public ValidationResult Validate(T instance, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(context);

        var failures = new List<ValidationFailure>();

        foreach (var rule in _compiledRules)
        {
            var value = rule.PropertyAccessor(instance);

            foreach (var check in rule.Checks)
            {
                if (!check.PropertyCheck.IsValid(value))
                {
                    var severity = ResolveSeverity(check, context.Layer);
                    var failure = new ValidationFailure(rule.PropertyName, check.Message, value)
                    {
                        ErrorCode = check.ErrorCode,
                        Severity = severity
                    };
                    failures.Add(failure);

                    if (CascadeMode == CascadeMode.StopOnFirstFailure)
                        return new ValidationResult(failures);
                }
            }
        }

        return new ValidationResult(failures);
    }

    #endregion

    #region IValidator (non-generic)

    /// <inheritdoc />
    Type IValidator.ValidatedType => typeof(T);

    /// <inheritdoc />
    ValidationResult IValidator.Validate(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (instance is not T typed)
            throw new ArgumentException(
                $"Expected instance of type '{typeof(T).FullName}' but received '{instance.GetType().FullName}'.",
                nameof(instance));
        return Validate(typed);
    }

    /// <inheritdoc />
    ValidationResult IValidator.Validate(object instance, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (instance is not T typed)
            throw new ArgumentException(
                $"Expected instance of type '{typeof(T).FullName}' but received '{instance.GetType().FullName}'.",
                nameof(instance));
        return Validate(typed, context);
    }

    #endregion

    #region Compilation

    private List<CompiledPropertyRule> CompileRules()
    {
        var rules = new List<CompiledPropertyRule>();
        var type = typeof(T);

        foreach (var (propertyName, propertyDef) in _definition.Properties)
        {
            var propInfo = type.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propInfo is null)
                throw new InvalidOperationException(
                    $"Property '{propertyName}' not found on type '{type.FullName}'.");

            var accessor = CreateAccessor(propInfo);
            var checks = new List<CompiledCheck>();

            foreach (var ruleDef in propertyDef.Rules)
            {
                var check = _registry.Resolve(ruleDef.Validator, ruleDef.Params);
                var defaultSeverity = SeverityParser.Parse(ruleDef.Severity);
                Dictionary<string, Severity>? layerSeverities = null;

                if (ruleDef.Layers is { Count: > 0 })
                {
                    layerSeverities = new Dictionary<string, Severity>(StringComparer.OrdinalIgnoreCase);
                    foreach (var (layer, severityStr) in ruleDef.Layers)
                    {
                        layerSeverities[layer] = SeverityParser.Parse(severityStr);
                    }
                }

                checks.Add(new CompiledCheck(
                    check,
                    ruleDef.Message,
                    ruleDef.ErrorCode,
                    defaultSeverity,
                    layerSeverities));
            }

            rules.Add(new CompiledPropertyRule(propInfo.Name, accessor, checks));
        }

        return rules;
    }

    private static Func<T, object?> CreateAccessor(PropertyInfo propInfo)
    {
        return instance => propInfo.GetValue(instance);
    }

    private static Severity ResolveSeverity(CompiledCheck check, string? layer)
    {
        if (layer is not null
            && check.LayerSeverities is not null
            && check.LayerSeverities.TryGetValue(layer, out var layerSeverity))
        {
            return layerSeverity;
        }

        return check.DefaultSeverity;
    }

    #endregion

    #region Inner types

    private sealed record CompiledPropertyRule(
        string PropertyName,
        Func<T, object?> PropertyAccessor,
        List<CompiledCheck> Checks);

    private sealed record CompiledCheck(
        IPropertyCheck PropertyCheck,
        string Message,
        string? ErrorCode,
        Severity DefaultSeverity,
        Dictionary<string, Severity>? LayerSeverities);

    #endregion
}
