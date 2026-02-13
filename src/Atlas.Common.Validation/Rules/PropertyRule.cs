using System.Linq.Expressions;
using Atlas.Common.Validation.Core;

namespace Atlas.Common.Validation.Rules;

/// <summary>
/// Represents a set of validation checks bound to a specific property of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
internal sealed class PropertyRule<T, TProperty> : IValidationRule<T>, IRuleBuilder<T, TProperty>
{
    private readonly Func<T, TProperty> _propertyAccessor;
    private readonly string _propertyName;
    private readonly List<CheckDescriptor> _checks = [];
    private Func<T, bool>? _currentCondition;
    private CascadeMode _cascadeMode = CascadeMode.Continue;

    /// <summary>
    /// Creates a new <see cref="PropertyRule{T, TProperty}"/> from a property expression.
    /// </summary>
    /// <param name="expression">The expression identifying the property to validate.</param>
    public PropertyRule(Expression<Func<T, TProperty>> expression)
    {
        _propertyAccessor = expression.Compile();
        _propertyName = GetPropertyName(expression: expression);
    }

    #region IRuleBuilder<T, TProperty>

    public IRuleBuilder<T, TProperty> AddCheck(Func<TProperty, bool> predicate, string defaultMessage)
    {
        _checks.Add(item: new CheckDescriptor(
            predicate: (_, val) => predicate(arg: val),
            errorMessage: defaultMessage,
            condition: _currentCondition));
        return this;
    }

    public IRuleBuilder<T, TProperty> AddCheck(Func<T, TProperty, bool> predicate, string defaultMessage)
    {
        _checks.Add(item: new CheckDescriptor(predicate: predicate, errorMessage: defaultMessage, condition: _currentCondition));
        return this;
    }

    public IRuleBuilder<T, TProperty> SetMessage(string message)
    {
        if (_checks.Count > 0)
            _checks[^1].ErrorMessage = message;
        return this;
    }

    public IRuleBuilder<T, TProperty> SetErrorCode(string errorCode)
    {
        if (_checks.Count > 0)
            _checks[^1].ErrorCode = errorCode;
        return this;
    }

    public IRuleBuilder<T, TProperty> SetSeverity(Severity severity)
    {
        if (_checks.Count > 0)
            _checks[^1].Severity = severity;
        return this;
    }

    public IRuleBuilder<T, TProperty> SetLayerSeverity(string layer, Severity severity)
    {
        if (_checks.Count > 0)
        {
            var check = _checks[^1];
            check.LayerSeverities ??= new Dictionary<string, Severity>(comparer: StringComparer.OrdinalIgnoreCase);
            check.LayerSeverities[key: layer] = severity;
        }
        return this;
    }

    public IRuleBuilder<T, TProperty> ApplyWhen(Func<T, bool> condition)
    {
        _currentCondition = condition;
        return this;
    }

    public IRuleBuilder<T, TProperty> ApplyUnless(Func<T, bool> condition)
    {
        _currentCondition = instance => !condition(arg: instance);
        return this;
    }

    public IRuleBuilder<T, TProperty> SetCascadeMode(CascadeMode cascadeMode)
    {
        _cascadeMode = cascadeMode;
        return this;
    }

    #endregion

    #region IValidationRule<T>

    /// <inheritdoc />
    public string PropertyName => _propertyName;

    public IEnumerable<ValidationFailure> Validate(T instance)
        => ValidateInternal(instance: instance, layer: null);

    public IEnumerable<ValidationFailure> Validate(T instance, IValidationContext context)
        => ValidateInternal(instance: instance, layer: context.Layer);

    private IEnumerable<ValidationFailure> ValidateInternal(T instance, string? layer)
    {
        var value = _propertyAccessor(arg: instance);

        foreach (var check in _checks)
        {
            // Skip check if a condition is set and evaluates to false
            if (check.Condition is not null && !check.Condition(arg: instance))
                continue;

            if (!check.Predicate(arg1: instance, arg2: value))
            {
                var severity = ResolveSeverity(check: check, layer: layer);

                var failure = new ValidationFailure(propertyName: _propertyName, errorMessage: check.ErrorMessage, attemptedValue: value)
                {
                    ErrorCode = check.ErrorCode,
                    Severity = severity
                };
                yield return failure;

                if (_cascadeMode == CascadeMode.StopOnFirstFailure)
                    yield break;
            }
        }
    }

    #endregion

    #region Helpers

    private static Severity ResolveSeverity(CheckDescriptor check, string? layer)
    {
        if (layer is not null
            && check.LayerSeverities is not null
            && check.LayerSeverities.TryGetValue(key: layer, value: out var layerSeverity))
        {
            return layerSeverity;
        }

        return check.Severity;
    }

    private static string GetPropertyName(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is UnaryExpression { Operand: MemberExpression unaryMember })
            return unaryMember.Member.Name;

        return expression.Body.ToString();
    }

    #endregion

    #region Inner types

    /// <summary>
    /// Describes a single validation check within a property rule.
    /// </summary>
    private sealed class CheckDescriptor
    {
        public Func<T, TProperty, bool> Predicate { get; }
        public string ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public Severity Severity { get; set; } = Severity.Forbidden;
        public Dictionary<string, Severity>? LayerSeverities { get; set; }
        public Func<T, bool>? Condition { get; }

        public CheckDescriptor(Func<T, TProperty, bool> predicate, string errorMessage, Func<T, bool>? condition)
        {
            Predicate = predicate;
            ErrorMessage = errorMessage;
            Condition = condition;
        }
    }

    #endregion
}
