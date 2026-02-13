using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Rules;

namespace Atlas.Common.Validation.Extensions;

/// <summary>
/// Extensions for customizing error messages, error codes, severity, and applying conditions to rules.
/// </summary>
public static class RuleMessageExtensions
{
    /// <summary>
    /// Sets a custom error message on the most recently added validation check.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="message">The custom error message.</param>
    public static IRuleBuilder<T, TProperty> WithMessage<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, string message)
    {
        return builder.SetMessage(message: message);
    }

    /// <summary>
    /// Sets a custom error code on the most recently added validation check.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="errorCode">The error code for programmatic handling.</param>
    public static IRuleBuilder<T, TProperty> WithErrorCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, string errorCode)
    {
        return builder.SetErrorCode(errorCode: errorCode);
    }

    /// <summary>
    /// Sets the severity level on the most recently added validation check.
    /// Defaults to <see cref="Severity.Forbidden"/> if not specified.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="severity">The severity level to assign.</param>
    public static IRuleBuilder<T, TProperty> WithSeverity<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Severity severity)
    {
        return builder.SetSeverity(severity: severity);
    }

    /// <summary>
    /// Sets a layer-specific severity override on the most recently added validation check.
    /// When validation runs within a context that specifies this layer,
    /// this severity will be used instead of the default.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="layer">The layer name (e.g. "api", "dto", "entity").</param>
    /// <param name="severity">The severity level for this layer.</param>
    public static IRuleBuilder<T, TProperty> WithLayerSeverity<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, string layer, Severity severity)
    {
        return builder.SetLayerSeverity(layer: layer, severity: severity);
    }

    /// <summary>
    /// Sets the cascade mode for this property rule.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="cascadeMode">The cascade mode to apply.</param>
    public static IRuleBuilder<T, TProperty> Cascade<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, CascadeMode cascadeMode)
    {
        return builder.SetCascadeMode(cascadeMode: cascadeMode);
    }

    /// <summary>
    /// Makes all subsequent checks in this rule conditional.
    /// Checks added after calling <see cref="When{T,TProperty}"/> will only execute
    /// when <paramref name="condition"/> returns <c>true</c>.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="condition">A predicate evaluated against the validated instance.</param>
    public static IRuleBuilder<T, TProperty> When<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, bool> condition)
    {
        return builder.ApplyWhen(condition: condition);
    }

    /// <summary>
    /// Makes all subsequent checks in this rule conditional.
    /// Checks added after calling <see cref="Unless{T,TProperty}"/> will be skipped
    /// when <paramref name="condition"/> returns <c>true</c>.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="condition">A predicate evaluated against the validated instance. If <c>true</c>, checks are skipped.</param>
    public static IRuleBuilder<T, TProperty> Unless<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, bool> condition)
    {
        return builder.ApplyUnless(condition: condition);
    }
}
