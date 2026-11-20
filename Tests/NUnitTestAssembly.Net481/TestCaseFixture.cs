namespace NUnitTestAssembly.Net481;

using System.Configuration;

using NUnit.Framework;

using NUnitTestAssembly.Common;

using Zat.Z2xxTests.Common.Model;

/// <summary>
/// Exercises common test-case outcomes and .NET Framework 4.8.1-specific configuration and bridge behavior.
/// </summary>
[TestFixture]
public class TestCaseFixture : TestCaseFixtureBase
{
    public const string ExpectedValue = "FROM TEST ASSEMBLY CONFIGURATION FILE";
    public const string ExpectedTestedRuntimeVersion = "1.2.3";
    public const string NoHwAssemblyMarker = "NONE";
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
    /// Reads the test config from the test runner over the bridge once per AppDomain, mirroring static
    /// <c>TestSettings</c> in <c>Zat.Z2xxTests</c>.
    /// </summary>
    [Test]
    public void UseTestRunnerBridgeTest()
    {
        var testConfig = StaticBridgeTestConfig.Value;

        Assert.That(testConfig.TestedRuntimeVersion, Is.EqualTo(ExpectedTestedRuntimeVersion));
        Assert.Pass(testConfig.TestedHwAssemblyType?.ToString() ?? NoHwAssemblyMarker);
    }

    private static class StaticBridgeTestConfig
    {
        static StaticBridgeTestConfig()
        {
            using var connection = Task
                .Run(() => TestRunnerBridgeConnector.ConnectAsync())
                .GetAwaiter()
                .GetResult();

            Value = Task
                .Run(connection.Bridge.GetTestConfigAsync)
                .GetAwaiter()
                .GetResult();
        }

        public static TestConfig Value { get; }
    }
}