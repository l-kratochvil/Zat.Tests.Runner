namespace Zat.Tests.Runner.WebApp.Shared.Validation;

using System.Linq;

using FluentValidation.Results;

/// <summary>
/// Turns what a FluentValidation run found into what the user interface shows.
/// </summary>
/// <remarks>
/// Kept in one place so that every validator written with FluentValidation reports a failure as the
/// same <see cref="Validity.Issue"/>, and so that the library reaches no further than the
/// validators themselves.
/// </remarks>
internal static class ValidationResultExtensions
{
    /// <summary>
    /// Reads the problems out of a validation run.
    /// </summary>
    /// <param name="result">What the validation run found.</param>
    /// <returns>The problems in <paramref name="result"/>, as the user interface reads them.</returns>
    internal static Validity ToValidity(this ValidationResult result)
        => result.IsValid
            ? Validity.Valid
            : new Validity(
                [
                    ..result.Errors.Select(
                        static failure => new Validity.Issue(
                            FieldName: failure.PropertyName,
                            Message: failure.ErrorMessage,
                            Severity: failure.Severity is FluentValidation.Severity.Error
                                ? Validity.Severity.Error
                                : Validity.Severity.Warning))
                ]);
}
