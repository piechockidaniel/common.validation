namespace Common.Validation.Rules;

/// <summary>
/// Fluent builder interface for constructing standalone validation rules on a value.
/// Unlike <see cref="IRuleBuilder{T, TProperty}"/>, this interface does not require
/// a parent object — the value is validated independently.
/// Extension methods on this interface provide the chainable validators.
/// </summary>
/// <typeparam name="TProperty">The type of the value being validated.</typeparam>
public interface IValueRuleBuilder<TProperty>
{
    /// <summary>
    /// Adds a validation check to the current value rule.
    /// </summary>
    /// <param name="predicate">A function that returns <c>true</c> if the value is valid.</param>
    /// <param name="defaultMessage">The default error message when the check fails.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> AddCheck(Func<TProperty, bool> predicate, string defaultMessage);

    /// <summary>
    /// Sets the error message on the most recently added check.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> SetMessage(string message);

    /// <summary>
    /// Sets the error code on the most recently added check.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> SetErrorCode(string errorCode);

    /// <summary>
    /// Sets the severity on the most recently added check.
    /// </summary>
    /// <param name="severity">The severity level.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> SetSeverity(Core.Severity severity);

    /// <summary>
    /// Sets a layer-specific severity override on the most recently added check.
    /// When validation runs within a context that specifies this layer,
    /// this severity will be used instead of the default.
    /// </summary>
    /// <param name="layer">The layer name (e.g. "api", "dto", "entity").</param>
    /// <param name="severity">The severity level for this layer.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> SetLayerSeverity(string layer, Core.Severity severity);

    /// <summary>
    /// Sets a condition under which all subsequent checks apply.
    /// The condition is evaluated against the value itself.
    /// </summary>
    /// <param name="condition">A predicate evaluated against the value.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> ApplyWhen(Func<TProperty, bool> condition);

    /// <summary>
    /// Sets a condition under which all subsequent checks are skipped.
    /// The condition is evaluated against the value itself.
    /// </summary>
    /// <param name="condition">A predicate evaluated against the value. If <c>true</c>, checks are skipped.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> ApplyUnless(Func<TProperty, bool> condition);

    /// <summary>
    /// Sets the cascade mode for this value rule.
    /// </summary>
    /// <param name="cascadeMode">The cascade mode to apply.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IValueRuleBuilder<TProperty> SetCascadeMode(Core.CascadeMode cascadeMode);
}
