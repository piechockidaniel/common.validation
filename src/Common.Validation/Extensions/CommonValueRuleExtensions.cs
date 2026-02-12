using Common.Validation.Rules;

namespace Common.Validation.Extensions;

/// <summary>
/// Common validation rule extensions for standalone <see cref="IValueRuleBuilder{TProperty}"/>.
/// These are the standalone equivalents of <see cref="CommonRuleExtensions"/>.
/// </summary>
public static class CommonValueRuleExtensions
{
    /// <summary>
    /// Validates that the value is not null.
    /// </summary>
    public static IValueRuleBuilder<TProperty> NotNull<TProperty>(this IValueRuleBuilder<TProperty> builder)
    {
        return builder.AddCheck(
            predicate: value => value is not null,
            defaultMessage: "must not be null.");
    }

    /// <summary>
    /// Validates that the value is null.
    /// </summary>
    public static IValueRuleBuilder<TProperty> Null<TProperty>(this IValueRuleBuilder<TProperty> builder)
    {
        return builder.AddCheck(
            predicate: value => value is null,
            defaultMessage: "must be null.");
    }

    /// <summary>
    /// Validates that a string is not null or empty, or that a collection is not empty.
    /// </summary>
    public static IValueRuleBuilder<TProperty> NotEmpty<TProperty>(this IValueRuleBuilder<TProperty> builder)
    {
        return builder.AddCheck(
            predicate: value => value switch
            {
                null => false,
                string s => !string.IsNullOrWhiteSpace(value: s),
                System.Collections.ICollection { Count: 0 } => false,
                _ => !EqualityComparer<TProperty>.Default.Equals(x: value, y: default!)
            },
            defaultMessage: "must not be empty.");
    }

    /// <summary>
    /// Validates that a string is null, empty, or whitespace, or that a value equals its default.
    /// </summary>
    public static IValueRuleBuilder<TProperty> Empty<TProperty>(this IValueRuleBuilder<TProperty> builder)
    {
        return builder.AddCheck(
            predicate: value => value switch
            {
                null => true,
                string s => string.IsNullOrWhiteSpace(value: s),
                System.Collections.ICollection { Count: 0 } => true,
                _ => EqualityComparer<TProperty>.Default.Equals(x: value, y: default!)
            },
            defaultMessage: "must be empty.");
    }

    /// <summary>
    /// Validates that the value equals the specified comparison value.
    /// </summary>
    public static IValueRuleBuilder<TProperty> Equal<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty comparisonValue)
    {
        return builder.AddCheck(
            predicate: value => EqualityComparer<TProperty>.Default.Equals(x: value, y: comparisonValue),
            defaultMessage: $"must equal '{comparisonValue}'.");
    }

    /// <summary>
    /// Validates that the value does not equal the specified comparison value.
    /// </summary>
    public static IValueRuleBuilder<TProperty> NotEqual<TProperty>(
        this IValueRuleBuilder<TProperty> builder, TProperty comparisonValue)
    {
        return builder.AddCheck(
            predicate: value => !EqualityComparer<TProperty>.Default.Equals(x: value, y: comparisonValue),
            defaultMessage: $"must not equal '{comparisonValue}'.");
    }

    /// <summary>
    /// Validates the value using a custom predicate.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="predicate">A function that returns <c>true</c> if the value is valid.</param>
    /// <param name="message">Optional custom error message.</param>
    public static IValueRuleBuilder<TProperty> Must<TProperty>(
        this IValueRuleBuilder<TProperty> builder,
        Func<TProperty, bool> predicate,
        string message = "did not satisfy the specified condition.")
    {
        return builder.AddCheck(predicate: predicate, defaultMessage: message);
    }
}
