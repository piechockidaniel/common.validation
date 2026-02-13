using System.Text.RegularExpressions;
using Atlas.Common.Validation.Rules;

namespace Atlas.Common.Validation.Extensions;

/// <summary>
/// String-specific validation rule extensions.
/// </summary>
public static partial class StringRuleExtensions
{
    // Precompiled regex patterns for common validations
    [GeneratedRegex(pattern: @"^[^@\s]+@[^@\s]+\.[^@\s]+$", options: RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 250)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(pattern: @"^\+?[\d\s\-\(\)]{7,20}$", options: RegexOptions.Compiled, matchTimeoutMilliseconds: 250)]
    private static partial Regex PhoneRegex();

    /// <summary>
    /// Validates that the string length is at most <paramref name="max"/> characters.
    /// </summary>
    public static IRuleBuilder<T, string> MaxLength<T>(
        this IRuleBuilder<T, string> builder, int max)
    {
        return builder.AddCheck(
            predicate: value => value is null || value.Length <= max,
            defaultMessage: $"must be at most {max} characters long.");
    }

    /// <summary>
    /// Validates that the string length is at least <paramref name="min"/> characters.
    /// </summary>
    public static IRuleBuilder<T, string> MinLength<T>(
        this IRuleBuilder<T, string> builder, int min)
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.Length >= min,
            defaultMessage: $"must be at least {min} characters long.");
    }

    /// <summary>
    /// Validates that the string length is between <paramref name="min"/> and <paramref name="max"/> characters (inclusive).
    /// </summary>
    public static IRuleBuilder<T, string> Length<T>(
        this IRuleBuilder<T, string> builder, int min, int max)
    {
        return builder.AddCheck(
            predicate: value => value is not null && value.Length >= min && value.Length <= max,
            defaultMessage: $"must be between {min} and {max} characters long.");
    }

    /// <summary>
    /// Validates that the string matches the specified regular expression pattern.
    /// </summary>
    public static IRuleBuilder<T, string> Matches<T>(
        this IRuleBuilder<T, string> builder, string pattern)
    {
        var regex = new Regex(pattern: pattern, options: RegexOptions.Compiled, matchTimeout: TimeSpan.FromMilliseconds(250));
        return builder.AddCheck(
            predicate: value => value is not null && regex.IsMatch(input: value),
            defaultMessage: $"must match the pattern '{pattern}'.");
    }

    /// <summary>
    /// Validates that the string matches the specified <see cref="Regex"/>.
    /// </summary>
    public static IRuleBuilder<T, string> Matches<T>(
        this IRuleBuilder<T, string> builder, Regex regex)
    {
        return builder.AddCheck(
            predicate: value => value is not null && regex.IsMatch(input: value),
            defaultMessage: "must match the required pattern.");
    }

    /// <summary>
    /// Validates that the string is a valid email address format.
    /// </summary>
    public static IRuleBuilder<T, string> EmailAddress<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder.AddCheck(
            predicate: value => value is not null && EmailRegex().IsMatch(input: value),
            defaultMessage: "must be a valid email address.");
    }

    /// <summary>
    /// Validates that the string is a valid phone number format.
    /// Accepts digits, spaces, dashes, parentheses, and an optional leading '+'.
    /// </summary>
    public static IRuleBuilder<T, string> PhoneNumber<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder.AddCheck(
            predicate: value => value is not null && PhoneRegex().IsMatch(input: value),
            defaultMessage: "must be a valid phone number.");
    }
}
