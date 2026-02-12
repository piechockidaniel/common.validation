using Common.Validation.Core;

namespace Common.Validation.Rules;

/// <summary>
/// Represents a set of validation checks bound to a standalone value.
/// Unlike <see cref="PropertyRule{T, TProperty}"/>, this rule does not depend
/// on a parent object — the value is validated independently.
/// </summary>
/// <typeparam name="TProperty">The type of the value being validated.</typeparam>
internal sealed class ValueRule<TProperty> : IValueValidationRule<TProperty>, IValueRuleBuilder<TProperty>
{
    private readonly string _propertyName;
    private readonly List<CheckDescriptor> _checks = [];
    private Func<TProperty, bool>? _currentCondition;
    private CascadeMode _cascadeMode = CascadeMode.Continue;

    /// <summary>
    /// Creates a new <see cref="ValueRule{TProperty}"/> with the specified property name.
    /// </summary>
    /// <param name="propertyName">The property name used in validation failure reports.</param>
    public ValueRule(string propertyName)
    {
        _propertyName = propertyName;
    }

    #region IValueRuleBuilder<TProperty>

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> AddCheck(Func<TProperty, bool> predicate, string defaultMessage)
    {
        _checks.Add(item: new CheckDescriptor(
            predicate: predicate,
            errorMessage: defaultMessage,
            condition: _currentCondition));
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> SetMessage(string message)
    {
        if (_checks.Count > 0)
            _checks[^1].ErrorMessage = message;
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> SetErrorCode(string errorCode)
    {
        if (_checks.Count > 0)
            _checks[^1].ErrorCode = errorCode;
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> SetSeverity(Severity severity)
    {
        if (_checks.Count > 0)
            _checks[^1].Severity = severity;
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> SetLayerSeverity(string layer, Severity severity)
    {
        if (_checks.Count > 0)
        {
            var check = _checks[^1];
            check.LayerSeverities ??= new Dictionary<string, Severity>(comparer: StringComparer.OrdinalIgnoreCase);
            check.LayerSeverities[key: layer] = severity;
        }
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> ApplyWhen(Func<TProperty, bool> condition)
    {
        _currentCondition = condition;
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> ApplyUnless(Func<TProperty, bool> condition)
    {
        _currentCondition = value => !condition(arg: value);
        return this;
    }

    /// <inheritdoc />
    public IValueRuleBuilder<TProperty> SetCascadeMode(CascadeMode cascadeMode)
    {
        _cascadeMode = cascadeMode;
        return this;
    }

    #endregion

    #region IValueValidationRule<TProperty>

    /// <inheritdoc />
    public string PropertyName => _propertyName;

    /// <inheritdoc />
    public IEnumerable<ValidationFailure> Validate(TProperty value)
        => ValidateInternal(value: value, layer: null);

    /// <inheritdoc />
    public IEnumerable<ValidationFailure> Validate(TProperty value, IValidationContext context)
        => ValidateInternal(value: value, layer: context.Layer);

    private IEnumerable<ValidationFailure> ValidateInternal(TProperty value, string? layer)
    {
        foreach (var check in _checks)
        {
            // Skip check if a condition is set and evaluates to false
            if (check.Condition is not null && !check.Condition(arg: value))
                continue;

            if (!check.Predicate(arg: value))
            {
                var severity = ResolveSeverity(check: check, layer: layer);

                var failure = new ValidationFailure(
                    propertyName: _propertyName,
                    errorMessage: check.ErrorMessage,
                    attemptedValue: value)
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

    #endregion

    #region Inner types

    /// <summary>
    /// Describes a single validation check within a value rule.
    /// </summary>
    private sealed class CheckDescriptor
    {
        public Func<TProperty, bool> Predicate { get; }
        public string ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public Severity Severity { get; set; } = Severity.Forbidden;
        public Dictionary<string, Severity>? LayerSeverities { get; set; }
        public Func<TProperty, bool>? Condition { get; }

        public CheckDescriptor(Func<TProperty, bool> predicate, string errorMessage, Func<TProperty, bool>? condition)
        {
            Predicate = predicate;
            ErrorMessage = errorMessage;
            Condition = condition;
        }
    }

    #endregion
}
