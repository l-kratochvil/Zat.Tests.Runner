namespace Zat.Tests.Runner.Common.Net.Services;

using System.Reflection;
using Microsoft.Extensions.Configuration;

/// <summary>
/// User secrets provider.
/// </summary>
public class SecretsProvider : ISecretsProvider
{
    private Secrets? secrets;

    /// <inheritdoc/>
    public Secrets GetSecrets()
        => this.secrets ??= LoadSecrets();

    private static Secrets LoadSecrets()
    {
        const string smtpAuthBaseKey = "SmtpAuth";
        const string smtpAuthUserNameKey = $"{smtpAuthBaseKey}:UserName";
        const string smtpAuthPasswordKey = $"{smtpAuthBaseKey}:Password";

        const string testLinkApiBaseKey = "TestLinkApi";
        const string testLinkApiApiKeyKey = $"{testLinkApiBaseKey}:ApiKey";

        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .Build();

        return new Secrets(
            new SmtpAuth(
                config[smtpAuthUserNameKey] ?? string.Empty,
                config[smtpAuthPasswordKey] ?? string.Empty),
            new TestLinkApi(
                config[testLinkApiApiKeyKey] ?? string.Empty));
    }
}