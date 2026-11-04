namespace Zat.Tests.Runner.Common.Net.Validation;

using System.Net.Mail;

using CSharpFunctionalExtensions;

/// <summary>
/// The rules an email address of a test run notification has to pass; shared by every app and the email result
/// handler, so all of them accept the very same addresses.
/// </summary>
public static class EmailAddressValidators
{
    private const string ZatDomain = "zat.cz";

    /// <summary>
    /// Checks whether <paramref name="address"/> is, as a whole, a single syntactically valid email address.
    /// </summary>
    /// <param name="address">The checked text.</param>
    /// <returns><see langword="true"/> when the text is a valid email address.</returns>
    public static bool IsValidEmail(string? address)
        => MailAddress.TryCreate(address, out var mailAddress)
           && mailAddress.Address == address;

    /// <summary>
    /// Checks whether the domain of <paramref name="address"/> is exactly <c>zat.cz</c>; subdomains don't pass.
    /// </summary>
    /// <param name="address">The checked email address.</param>
    /// <returns><see langword="true"/> when the address belongs to the <c>zat.cz</c> domain.</returns>
    public static bool IsInZatDomain(string address)
    {
        var atIndex = address.LastIndexOf('@');

        return atIndex >= 0
               && string.Equals(address[(atIndex + 1)..], ZatDomain, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether <paramref name="address"/> is a valid email address in the <c>zat.cz</c> domain.
    /// </summary>
    /// <param name="address">The checked email address.</param>
    /// <returns>A success, or a failure with the message for the tester.</returns>
    public static Result Validate(string address)
    {
        if (!IsValidEmail(address))
        {
            return Result.Failure("Enter a valid email address.");
        }

        return IsInZatDomain(address)
            ? Result.Success()
            : Result.Failure("Use an email address in the zat.cz domain.");
    }

    /// <summary>
    /// Checks whether <paramref name="address"/> passes <see cref="Validate"/> and is not in <paramref name="existing"/> yet.
    /// </summary>
    /// <param name="address">The checked email address.</param>
    /// <param name="existing">The addresses already listed; compared case-insensitively.</param>
    /// <returns>A success, or a failure with the message for the tester.</returns>
    public static Result ValidateNew(string address, IReadOnlyList<string> existing)
        => Validate(address)
            .Ensure(
                () => !existing.Contains(address, StringComparer.OrdinalIgnoreCase),
                "This email address has already been added.");
}