using Atlas.Common.Validation.Core;
using Atlas.Common.Validation.Rules;

namespace Atlas.Common.Validation.Extensions;

/// <summary>
/// Extensions for customizing error messages, error codes, severity, and applying conditions
/// to standalone value rules.
/// These are the standalone equivalents of <see cref="RuleMessageExtensions"/>.
/// </summary>
public static class ValueRuleMessageExtensions
{
    /// <summary>
    /// Sets a custom error message on the most recently added validation check.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="message">The custom error message.</param>
    public static IValueRuleBuilder<TProperty> WithMessage<TProperty>(
        this IValueRuleBuilder<TProperty> builder, string message)
    {
        return builder.SetMessage(message: message);
    }

    /// <summary>
    /// Sets a custom error code on the most recently added validation check.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="errorCode">The error code for programmatic handling.</param>
    public static IValueRuleBuilder<TProperty> WithErrorCode<TProperty>(
        this IValueRuleBuilder<TProperty> builder, string errorCode)
    {
        return builder.SetErrorCode(errorCode: errorCode);
    }

    /// <summary>
    /// Sets the severity level on the most recently added validation check.
    /// Defaults to <see cref="Severity.Forbidden"/> if not specified.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="severity">The severity level to assign.</param>
    public static IValueRuleBuilder<TProperty> WithSeverity<TProperty>(
        this IValueRuleBuilder<TProperty> builder, Severity severity)
    {
        return builder.SetSeverity(severity: severity);
    }

    /// <summary>
    /// Sets a layer-specific severity override on the most recently added validation check.
    /// When validation runs within a context that specifies this layer,
    /// this severity will be used instead of the default.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="layer">The layer name (e.g. "api", "dto", "entity").</param>
    /// <param name="severity">The severity level for this layer.</param>
    public static IValueRuleBuilder<TProperty> WithLayerSeverity<TProperty>(
        this IValueRuleBuilder<TProperty> builder, string layer, Severity severity)
    {
        return builder.SetLayerSeverity(layer: layer, severity: severity);
    }

    /// <summary>
    /// Sets the cascade mode for this value rule.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="cascadeMode">The cascade mode to apply.</param>
    public static IValueRuleBuilder<TProperty> Cascade<TProperty>(
        this IValueRuleBuilder<TProperty> builder, CascadeMode cascadeMode)
    {
        return builder.SetCascadeMode(cascadeMode: cascadeMode);
    }

    /// <summary>
    /// Makes all subsequent checks in this rule conditional.
    /// Checks added after calling <see cref="When{TProperty}"/> will only execute
    /// when <paramref name="condition"/> returns <c>true</c>.
    /// The condition is evaluated against the value itself, not a parent object.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="condition">A predicate evaluated against the value.</param>
    public static IValueRuleBuilder<TProperty> When<TProperty>(
        this IValueRuleBuilder<TProperty> builder, Func<TProperty, bool> condition)
    {
        return builder.ApplyWhen(condition: condition);
    }

    /// <summary>
    /// Makes all subsequent checks in this rule conditional.
    /// Checks added after calling <see cref="Unless{TProperty}"/> will be skipped
    /// when <paramref name="condition"/> returns <c>true</c>.
    /// The condition is evaluated against the value itself, not a parent object.
    /// </summary>
    /// <param name="builder">The value rule builder.</param>
    /// <param name="condition">A predicate evaluated against the value. If <c>true</c>, checks are skipped.</param>
    public static IValueRuleBuilder<TProperty> Unless<TProperty>(
        this IValueRuleBuilder<TProperty> builder, Func<TProperty, bool> condition)
    {
        return builder.ApplyUnless(condition: condition);
    }
}
