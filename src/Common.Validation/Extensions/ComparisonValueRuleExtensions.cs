using Common.Validation.Rules;

namespace Common.Validation.Extensions;

/// <summary>
/// Comparison-based validation rule extensions for standalone <see cref="IValueRuleBuilder{TProperty}"/>
/// with <see cref="IComparable{T}"/> types.
/// These are the standalone equivalents of <see cref="ComparisonRuleExtensions"/>.
/// </summary>
public static class ComparisonValueRuleExtensions
{
    /// <summary>
    /// Validates that the value is greater than the specified threshold.
    /// </summary>
    public static IValueRuleBuilder<TProperty> GreaterThan<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.CompareTo(other: threshold) > 0,
            defaultMessage: $"must be greater than {threshold}.");
    }

    /// <summary>
    /// Validates that the value is greater than or equal to the specified threshold.
    /// </summary>
    public static IValueRuleBuilder<TProperty> GreaterThanOrEqual<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.CompareTo(other: threshold) >= 0,
            defaultMessage: $"must be greater than or equal to {threshold}.");
    }

    /// <summary>
    /// Validates that the value is less than the specified threshold.
    /// </summary>
    public static IValueRuleBuilder<TProperty> LessThan<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.CompareTo(other: threshold) < 0,
            defaultMessage: $"must be less than {threshold}.");
    }

    /// <summary>
    /// Validates that the value is less than or equal to the specified threshold.
    /// </summary>
    public static IValueRuleBuilder<TProperty> LessThanOrEqual<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.CompareTo(other: threshold) <= 0,
            defaultMessage: $"must be less than or equal to {threshold}.");
    }

    /// <summary>
    /// Validates that the value is between <paramref name="from"/> and <paramref name="to"/> (inclusive).
    /// </summary>
    public static IValueRuleBuilder<TProperty> InclusiveBetween<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty from, TProperty to)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.CompareTo(other: from) >= 0 && value.CompareTo(other: to) <= 0,
            defaultMessage: $"must be between {from} and {to} (inclusive).");
    }
}
