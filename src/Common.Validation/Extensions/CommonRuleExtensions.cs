using Common.Validation.Rules;

namespace Common.Validation.Extensions;

/// <summary>
/// Common validation rule extensions applicable to most property types.
/// </summary>
public static class CommonRuleExtensions
{
    /// <summary>
    /// Validates that the value is not null.
    /// </summary>
    public static IRuleBuilder<T, TProperty> NotNull<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
    {
        return builder.AddCheck(
            predicate: value => value is not null,
            defaultMessage: "must not be null.");
    }

    /// <summary>
    /// Validates that the value is null.
    /// </summary>
    public static IRuleBuilder<T, TProperty> Null<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
    {
        return builder.AddCheck(
            predicate: value => value is null,
            defaultMessage: "must be null.");
    }

    /// <summary>
    /// Validates that a string is not null or empty, or that a collection is not empty.
    /// </summary>
    public static IRuleBuilder<T, TProperty> NotEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
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
    public static IRuleBuilder<T, TProperty> Empty<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
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
    public static IRuleBuilder<T, TProperty> Equal<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty comparisonValue)
    {
        return builder.AddCheck(
            predicate: value => EqualityComparer<TProperty>.Default.Equals(x: value, y: comparisonValue),
            defaultMessage: $"must equal '{comparisonValue}'.");
    }

    /// <summary>
    /// Validates that the value does not equal the specified comparison value.
    /// </summary>
    public static IRuleBuilder<T, TProperty> NotEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty comparisonValue)
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
    public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder,
        Func<TProperty, bool> predicate,
        string message = "did not satisfy the specified condition.")
    {
        return builder.AddCheck(predicate: predicate, defaultMessage: message);
    }

    /// <summary>
    /// Validates the value using a custom predicate that also receives the parent instance.
    /// </summary>
    /// <param name="builder">The rule builder.</param>
    /// <param name="predicate">A function that receives the instance and the value and returns <c>true</c> if valid.</param>
    /// <param name="message">Optional custom error message.</param>
    public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder,
        Func<T, TProperty, bool> predicate,
        string message = "did not satisfy the specified condition.")
    {
        return builder.AddCheck(predicate: predicate, defaultMessage: message);
    }
}
