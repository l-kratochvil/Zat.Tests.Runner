namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Represents the user secrets.
/// </summary>
/// <param name="SmtpAuth">The SMTP authentication credentials.</param>
/// <param name="TestLinkApi">The TestLink API secrets.</param>
public record Secrets(
    SmtpAuth SmtpAuth,
    TestLinkApi TestLinkApi);

/// <summary>
/// Represents SMTP authentication credentials.
/// </summary>
/// <param name="UserName">The SMTP username.</param>
/// <param name="Password">The SMTP password.</param>
public record SmtpAuth(
    string UserName,
    string Password);

/// <summary>
/// Represents TestLink configuration.
/// </summary>
/// <param name="ApiKey">The TestLink API key.</param>
public record TestLinkApi(
    string ApiKey);