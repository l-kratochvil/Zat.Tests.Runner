namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// A test fixture that passes only when the test runner serves its bridge, the way Zat.Z2xxTests reads its test config.
/// </summary>
[TestFixture]
public class TestRunnerBridgeFixture
{
    /// <summary>
    /// The tested runtime version the test runner is expected to serve over the bridge.
    /// </summary>
    public const string ExpectedTestedRuntimeVersion = "1.2.3";

    /// <summary>
    /// Reads the test config from the test runner over the bridge.
    /// </summary>
    [Test]
    public async Task ReadsTestConfigFromTestRunner()
    {
        using var connection = await TestRunnerBridgeConnector.ConnectAsync();

        var testConfig = await connection.Bridge.GetTestConfigAsync();

        Assert.That(testConfig.TestedRuntimeVersion, Is.EqualTo(ExpectedTestedRuntimeVersion));
    }
}