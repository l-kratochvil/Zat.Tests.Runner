namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using System.Diagnostics;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Services;

[TestFixture]
public class NUnitTestRunnerProxyConnectorTests
{
    private NUnitTestRunnerProxyConnector unit = null!;

    [SetUp]
    public void SetUp()
        => this.unit = new NUnitTestRunnerProxyConnector(launchDebugger: false);

    [Test]
    public async Task ConnectAsync__WhenConnectionIsDisposed__ThenShouldEndProxyServerProcess()
    {
        // Given:
        var connection = (NUnitTestRunnerProxyConnection)await this.unit.ConnectAsync();
        using var serverProcess = Process.GetProcessById(connection.ServerProcessId);

        // When:
        await connection.DisposeAsync();

        // Then:
        Assert.That(serverProcess.HasExited, Is.True);
    }
}