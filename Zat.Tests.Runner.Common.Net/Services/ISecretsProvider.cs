namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Describes a provider for user secrets.
/// </summary>
public interface ISecretsProvider
{
    /// <summary>
    /// Gets the secrets.
    /// </summary>
    /// <returns>The secrets.</returns>
    Secrets GetSecrets();
}