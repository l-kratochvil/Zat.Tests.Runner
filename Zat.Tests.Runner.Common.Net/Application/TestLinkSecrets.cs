namespace Zat.Tests.Runner.Common.Net.Application;

/// <summary>
/// TestLink secrets, bound from the <see cref="SectionName"/> configuration section.
/// </summary>
public class TestLinkSecrets
{
    /// <summary>
    /// The configuration section the secrets are bound from.
    /// </summary>
    public const string SectionName = "TestLink";

    /// <summary>
    /// Gets the TestLink API key.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
}