namespace Zat.Tests.Runner.Common.Net.Application;

/// <summary>
/// SMTP authentication credentials, bound from the <see cref="SectionName"/> configuration section.
/// </summary>
public class SmtpAuthSecrets
{
    /// <summary>
    /// The configuration section the credentials are bound from.
    /// </summary>
    public const string SectionName = "SmtpAuth";

    /// <summary>
    /// Gets the SMTP user name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SMTP password.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}