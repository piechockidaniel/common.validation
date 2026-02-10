namespace Common.Validation.Rules;

/// <summary>
/// Fluent builder interface for constructing validation rules on a specific property.
/// Extension methods on this interface provide the chainable validators.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
public interface IRuleBuilder<T, TProperty>
{
    /// <summary>
    /// Adds a validation check to the current property rule.
    /// </summary>
    /// <param name="predicate">A function that returns <c>true</c> if the value is valid.</param>
    /// <param name="defaultMessage">The default error message when the check fails.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> AddCheck(Func<TProperty, bool> predicate, string defaultMessage);

    /// <summary>
    /// Adds a validation check that has access to the parent object instance.
    /// </summary>
    /// <param name="predicate">A function that takes the instance and property value and returns <c>true</c> if valid.</param>
    /// <param name="defaultMessage">The default error message when the check fails.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> AddCheck(Func<T, TProperty, bool> predicate, string defaultMessage);

    /// <summary>
    /// Sets the error message on the most recently added check.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> SetMessage(string message);

    /// <summary>
    /// Sets the error code on the most recently added check.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> SetErrorCode(string errorCode);

    /// <summary>
    /// Sets the severity on the most recently added check.
    /// </summary>
    /// <param name="severity">The severity level.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> SetSeverity(Core.Severity severity);

    /// <summary>
    /// Sets a layer-specific severity override on the most recently added check.
    /// When validation runs within a context that specifies this layer,
    /// this severity will be used instead of the default.
    /// </summary>
    /// <param name="layer">The layer name (e.g. "api", "dto", "entity").</param>
    /// <param name="severity">The severity level for this layer.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> SetLayerSeverity(string layer, Core.Severity severity);

    /// <summary>
    /// Sets a condition under which all subsequent checks apply.
    /// </summary>
    /// <param name="condition">A predicate evaluated against the instance.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> ApplyWhen(Func<T, bool> condition);

    /// <summary>
    /// Sets a condition under which all subsequent checks are skipped.
    /// </summary>
    /// <param name="condition">A predicate evaluated against the instance. If <c>true</c>, checks are skipped.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> ApplyUnless(Func<T, bool> condition);

    /// <summary>
    /// Sets the cascade mode for this property rule.
    /// </summary>
    /// <param name="cascadeMode">The cascade mode to apply.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    IRuleBuilder<T, TProperty> SetCascadeMode(Core.CascadeMode cascadeMode);
}
