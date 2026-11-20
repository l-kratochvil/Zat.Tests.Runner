namespace Zat.Tests.Runner.WebApp.Shared.Validation;

using System.Linq;

/// <summary>
/// The issues found in something, in the order the fields are shown in.
/// </summary>
/// <remarks>
/// Every validator in the application answers in this, so a second feature does not invent a third
/// way of saying that a field is wrong.
/// </remarks>
/// <param name="Issues">Issues found.</param>
public sealed record Validity(IReadOnlyList<Validity.Issue> Issues)
{
    /// <summary>
    /// How serious a <see cref="Issue"/> is.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// The value cannot be used.
        /// </summary>
        Error,

        /// <summary>
        /// The value can be used, but something about it is worth knowing.
        /// </summary>
        Warning,
    }

    /// <summary>
    /// Gets a valid validity.
    /// </summary>
    public static Validity Valid
        => new([]);

    /// <summary>
    /// Gets a value indicating whether what was validated has errors.
    /// </summary>
    public bool HasErrors
        => this.Issues.Any(
            static problem => problem.Severity is Severity.Error);

    /// <summary>
    /// Gets a value indicating whether what was validated has warnings.
    /// </summary>
    public bool HasWarnings
        => this.Issues.Any(
            static problem => problem.Severity is Severity.Warning);

    /// <summary>
    /// Narrows this down to one field.
    /// </summary>
    /// <param name="fieldName">Field to ask about.</param>
    /// <returns>The <see cref="Validity"/> of <paramref name="fieldName"/> alone.</returns>
    public Validity For(string fieldName)
        => new(
        [
            ..this.Issues.Where(
                issue => string.Equals(issue.FieldName, fieldName, StringComparison.Ordinal))
        ]);

    /// <summary>
    /// One issue found in what was validated.
    /// </summary>
    /// <param name="FieldName">Name of the field the issue belongs to.</param>
    /// <param name="Message">Issue text shown to the tester.</param>
    /// <param name="Severity">How serious the issue is.</param>
    public sealed record Issue(
        string FieldName,
        string Message,
        Severity Severity = Severity.Error);
}