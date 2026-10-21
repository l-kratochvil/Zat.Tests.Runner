namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Represents the user secrets.
/// </summary>
/// <param name="SmtpAuth">The SMTP authentication credentials.</param>
public record Secrets(
    SmtpAuth SmtpAuth);

/// <summary>
/// Represents SMTP authentication credentials.
/// </summary>
/// <param name="UserName">The SMTP username.</param>
/// <param name="Password">The SMTP password.</param>
public record SmtpAuth(
    string UserName,
    string Password);