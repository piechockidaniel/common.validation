namespace Common.Validation.Core;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    private readonly List<ValidationFailure> _errors;

    /// <summary>
    /// Creates a new <see cref="ValidationResult"/> with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of validation failures.</param>
    public ValidationResult(IEnumerable<ValidationFailure> errors)
    {
        _errors = new List<ValidationFailure>(errors);
    }

    /// <summary>
    /// Creates a new empty (valid) <see cref="ValidationResult"/>.
    /// </summary>
    public ValidationResult() : this(Enumerable.Empty<ValidationFailure>()) { }

    /// <summary>
    /// Gets a value indicating whether the validation was successful (no errors).
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Gets the collection of validation failures.
    /// </summary>
    public IReadOnlyList<ValidationFailure> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Gets whether there are any <see cref="Severity.Forbidden"/> failures.
    /// When <c>true</c>, the operation must not proceed.
    /// </summary>
    public bool HasForbidden => _errors.Exists(e => e.Severity == Severity.Forbidden);

    /// <summary>
    /// Gets whether there are any <see cref="Severity.AtOwnRisk"/> failures.
    /// </summary>
    public bool HasAtOwnRisk => _errors.Exists(e => e.Severity == Severity.AtOwnRisk);

    /// <summary>
    /// Gets whether there are any <see cref="Severity.NotRecommended"/> failures.
    /// </summary>
    public bool HasNotRecommended => _errors.Exists(e => e.Severity == Severity.NotRecommended);

    /// <summary>
    /// Returns only failures of the specified severity.
    /// </summary>
    public IReadOnlyList<ValidationFailure> BySeverity(Severity severity)
        => _errors.Where(e => e.Severity == severity).ToList().AsReadOnly();

    /// <summary>
    /// Merges another <see cref="ValidationResult"/> into this one,
    /// returning a new <see cref="ValidationResult"/> containing all failures from both.
    /// </summary>
    /// <param name="other">The other result to merge.</param>
    /// <returns>A new <see cref="ValidationResult"/> with combined failures.</returns>
    public ValidationResult Merge(ValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var combined = new List<ValidationFailure>(_errors.Count + other._errors.Count);
        combined.AddRange(_errors);
        combined.AddRange(other._errors);
        return new ValidationResult(combined);
    }

    /// <summary>
    /// Combines multiple <see cref="ValidationResult"/> instances into a single result.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A new <see cref="ValidationResult"/> with all failures from all results.</returns>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var combined = new List<ValidationFailure>();
        foreach (var result in results)
        {
            combined.AddRange(result._errors);
        }
        return new ValidationResult(combined);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsValid)
            return "Validation succeeded.";

        return $"Validation failed with {_errors.Count} error(s):{Environment.NewLine}" +
               string.Join(Environment.NewLine, _errors.Select(e => $"  - {e}"));
    }
}
