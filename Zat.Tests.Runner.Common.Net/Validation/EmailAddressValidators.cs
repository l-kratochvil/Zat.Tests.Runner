namespace Zat.Tests.Runner.Common.Net.Validation;

using System.Net.Mail;

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
}