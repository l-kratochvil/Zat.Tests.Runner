namespace NUnitTestAssembly.Net481;

using System.Configuration;

using NUnit.Framework;

using NUnitTestAssembly.Common;

/// <summary>
/// Exercises common test-case outcomes and .NET Framework 4.8.1-specific configuration and bridge behavior.
/// </summary>
[TestFixture]
public class TestCaseFixture : TestCaseFixtureBase
{
    public const string ExpectedValue = "FROM TEST ASSEMBLY CONFIGURATION FILE";
    public const string ExpectedTestedRuntimeVersion = "1.2.3";
    public const string ConfigurationValueKey = "TestCaseFixture.Value";

    /// <summary>
    /// Reads a value from the test assembly configuration file.
    /// </summary>
    [Test]
    public void UseConfigurationFileTest()
    {
        Assert.That(ConfigurationManager.AppSettings[ConfigurationValueKey], Is.EqualTo(ExpectedValue));
    }

    /// <summary>
    /// Reads the test config from the test runner over the bridge.
    /// </summary>
    [Test]
    public async Task UseTestRunnerBridgeTest()
    {
        using var connection = await TestRunnerBridgeConnector.ConnectAsync();

        var testConfig = await connection.Bridge.GetTestConfigAsync();

        Assert.That(testConfig.TestedRuntimeVersion, Is.EqualTo(ExpectedTestedRuntimeVersion));
    }
}