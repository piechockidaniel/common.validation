using Common.Validation.Rules;

namespace Common.Validation.Extensions;

/// <summary>
/// Comparison-based validation rule extensions for <see cref="IComparable{T}"/> types.
/// </summary>
public static class ComparisonRuleExtensions
{
    /// <summary>
    /// Validates that the value is greater than the specified threshold.
    /// </summary>
    public static IRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            value => value is not null && value.CompareTo(threshold) > 0,
            $"must be greater than {threshold}.");
    }

    /// <summary>
    /// Validates that the value is greater than or equal to the specified threshold.
    /// </summary>
    public static IRuleBuilder<T, TProperty> GreaterThanOrEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            value => value is not null && value.CompareTo(threshold) >= 0,
            $"must be greater than or equal to {threshold}.");
    }

    /// <summary>
    /// Validates that the value is less than the specified threshold.
    /// </summary>
    public static IRuleBuilder<T, TProperty> LessThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            value => value is not null && value.CompareTo(threshold) < 0,
            $"must be less than {threshold}.");
    }

    /// <summary>
    /// Validates that the value is less than or equal to the specified threshold.
    /// </summary>
    public static IRuleBuilder<T, TProperty> LessThanOrEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            value => value is not null && value.CompareTo(threshold) <= 0,
            $"must be less than or equal to {threshold}.");
    }

    /// <summary>
    /// Validates that the value is between <paramref name="from"/> and <paramref name="to"/> (inclusive).
    /// </summary>
    public static IRuleBuilder<T, TProperty> InclusiveBetween<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty from, TProperty to)
        where TProperty : IComparable<TProperty>
    {
        return builder.AddCheck(
            value => value is not null && value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0,
            $"must be between {from} and {to} (inclusive).");
    }
}
