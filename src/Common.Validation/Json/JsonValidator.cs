using System.Linq.Expressions;
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
public class JsonValidator<T> : IPropertyValidator<T>
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
        ArgumentNullException.ThrowIfNull(argument: definition);
        _definition = definition;
        _registry = registry ?? new ValidatorTypeRegistry();
        _compiledRules = CompileRules();
    }

    #region IValidator<T>

    /// <inheritdoc />
    public ValidationResult Validate(T instance)
    {
        return Validate(instance: instance, context: new ValidationContext(layer: CachedLayer));
    }

    /// <inheritdoc />
    public ValidationResult Validate(T instance, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);
        ArgumentNullException.ThrowIfNull(argument: context);

        var failures = new List<ValidationFailure>();

        foreach (var rule in _compiledRules)
        {
            var value = rule.PropertyAccessor(arg: instance);

            foreach (var check in rule.Checks.Where(predicate: c => !c.PropertyCheck.IsValid(value: value)))
            {
                var severity = ResolveSeverity(check: check, layer: context.Layer);
                var failure = new ValidationFailure(propertyName: rule.PropertyName, errorMessage: check.Message, attemptedValue: value)
                {
                    ErrorCode = check.ErrorCode,
                    Severity = severity
                };
                failures.Add(item: failure);

                if (CascadeMode == CascadeMode.StopOnFirstFailure)
                    return new ValidationResult(errors: failures);
            }
        }

        return new ValidationResult(errors: failures);
    }

    /// <inheritdoc cref="IPropertyValidator{T}.ValidateProperty" />
    ValidationResult IPropertyValidator<T>.ValidateProperty<TProperty>(T instance, Expression<Func<T, TProperty>> propertyExpression, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);
        ArgumentNullException.ThrowIfNull(argument: propertyExpression);
        ArgumentNullException.ThrowIfNull(argument: context);

        var propertyName = PropertyExpressionHelper.GetPropertyName(expression: propertyExpression);
        var failures = new List<ValidationFailure>();

        foreach (var rule in _compiledRules)
        {
            if (!string.Equals(rule.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            var value = rule.PropertyAccessor(arg: instance);

            foreach (var check in rule.Checks.Where(predicate: c => !c.PropertyCheck.IsValid(value: value)))
            {
                var severity = ResolveSeverity(check: check, layer: context.Layer);
                var failure = new ValidationFailure(propertyName: rule.PropertyName, errorMessage: check.Message, attemptedValue: value)
                {
                    ErrorCode = check.ErrorCode,
                    Severity = severity
                };
                failures.Add(item: failure);

                if (CascadeMode == CascadeMode.StopOnFirstFailure)
                    return new ValidationResult(errors: failures);
            }
        }

        return new ValidationResult(errors: failures);
    }

    #endregion

    #region IValidator (non-generic)

    /// <inheritdoc />
    Type IValidator.ValidatedType => typeof(T);

    /// <inheritdoc />
    ValidationResult IValidator.Validate(object instance)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);
        if (instance is not T typed)
            throw new ArgumentException(
                message: $"Expected instance of type '{typeof(T).FullName}' but received '{instance.GetType().FullName}'.",
                paramName: nameof(instance));
        return Validate(instance: typed);
    }

    /// <inheritdoc />
    ValidationResult IValidator.Validate(object instance, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: instance);
        if (instance is not T typed)
            throw new ArgumentException(
                message: $"Expected instance of type '{typeof(T).FullName}' but received '{instance.GetType().FullName}'.",
                paramName: nameof(instance));
        return Validate(instance: typed, context: context);
    }

    #endregion

    #region Compilation

    private List<CompiledPropertyRule> CompileRules()
    {
        var rules = new List<CompiledPropertyRule>();
        var type = typeof(T);

        if (_definition.Properties is null)
        {
            return rules;
        }

        foreach (var (propertyName, propertyDef) in _definition.Properties)
        {
            var propInfo = type.GetProperty(name: propertyName,
                bindingAttr: BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propInfo is null)
                throw new InvalidOperationException(
                    message: $"Property '{propertyName}' not found on type '{type.FullName}'.");

            var accessor = CreateAccessor(propInfo: propInfo);
            var checks = new List<CompiledCheck>();

            if (propertyDef.Rules is not null)
            {
                foreach (var ruleDef in propertyDef.Rules)
                {
                    var check = _registry.Resolve(name: ruleDef.Validator, parameters: ruleDef.Params);
                    var defaultSeverity = SeverityParser.Parse(value: ruleDef.Severity);
                    Dictionary<string, Severity>? layerSeverities = null;

                    if (ruleDef.Layers is { Count: > 0 })
                    {
                        layerSeverities = new Dictionary<string, Severity>(comparer: StringComparer.OrdinalIgnoreCase);
                        foreach (var (layer, severityStr) in ruleDef.Layers)
                        {
                            layerSeverities[key: layer] = SeverityParser.Parse(value: severityStr);
                        }
                    }

                    checks.Add(item: new CompiledCheck(
                        PropertyCheck: check,
                        Message: ruleDef.Message,
                        ErrorCode: ruleDef.ErrorCode,
                        DefaultSeverity: defaultSeverity,
                        LayerSeverities: layerSeverities));
                }
            }

            rules.Add(item: new CompiledPropertyRule(PropertyName: propInfo.Name, PropertyAccessor: accessor, Checks: checks));
        }

        return rules;
    }

    private static Func<T, object?> CreateAccessor(PropertyInfo propInfo)
    {
        return instance => propInfo.GetValue(obj: instance);
    }

    private static Severity ResolveSeverity(CompiledCheck check, string? layer)
    {
        if (layer is not null
            && check.LayerSeverities is not null
            && check.LayerSeverities.TryGetValue(key: layer, value: out var layerSeverity))
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
