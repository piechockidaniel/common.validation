using System.Linq.Expressions;
using Common.Validation.Core;

namespace Common.Validation.Rules;

/// <summary>
/// Represents a set of validation checks bound to a specific property of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
internal sealed class PropertyRule<T, TProperty> : IValidationRule<T>, IRuleBuilder<T, TProperty>
{
    private readonly Func<T, TProperty> _propertyAccessor;
    private readonly string _propertyName;
    private readonly List<CheckDescriptor> _checks = new();
    private Func<T, bool>? _currentCondition;

    /// <summary>
    /// Creates a new <see cref="PropertyRule{T, TProperty}"/> from a property expression.
    /// </summary>
    /// <param name="expression">The expression identifying the property to validate.</param>
    public PropertyRule(Expression<Func<T, TProperty>> expression)
    {
        _propertyAccessor = expression.Compile();
        _propertyName = GetPropertyName(expression);
    }

    #region IRuleBuilder<T, TProperty>

    public IRuleBuilder<T, TProperty> AddCheck(Func<TProperty, bool> predicate, string defaultMessage)
    {
        _checks.Add(new CheckDescriptor(
            (_, val) => predicate(val),
            defaultMessage,
            _currentCondition));
        return this;
    }

    public IRuleBuilder<T, TProperty> AddCheck(Func<T, TProperty, bool> predicate, string defaultMessage)
    {
        _checks.Add(new CheckDescriptor(predicate, defaultMessage, _currentCondition));
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

    public IRuleBuilder<T, TProperty> SetSeverity(Core.Severity severity)
    {
        if (_checks.Count > 0)
            _checks[^1].Severity = severity;
        return this;
    }

    public IRuleBuilder<T, TProperty> ApplyWhen(Func<T, bool> condition)
    {
        _currentCondition = condition;
        return this;
    }

    public IRuleBuilder<T, TProperty> ApplyUnless(Func<T, bool> condition)
    {
        _currentCondition = instance => !condition(instance);
        return this;
    }

    #endregion

    #region IValidationRule<T>

    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        var value = _propertyAccessor(instance);

        foreach (var check in _checks)
        {
            // Skip check if a condition is set and evaluates to false
            if (check.Condition is not null && !check.Condition(instance))
                continue;

            if (!check.Predicate(instance, value))
            {
                var failure = new ValidationFailure(_propertyName, check.ErrorMessage, value)
                {
                    ErrorCode = check.ErrorCode,
                    Severity = check.Severity
                };
                yield return failure;
            }
        }
    }

    #endregion

    #region Helpers

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
        public Core.Severity Severity { get; set; } = Core.Severity.Forbidden;
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
