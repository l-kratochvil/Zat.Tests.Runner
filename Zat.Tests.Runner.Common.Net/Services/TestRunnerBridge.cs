namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Z2xxTests.Common.Model;
using Zat.Z2xxTests.Common.Services;

public class TestRunnerBridge(
    TestConfig testConfig)
    : ITestRunnerBridge
{
    /// <inheritdoc/>
    public Task<TestConfig> GetTestConfigAsync()
        => Task.FromResult(testConfig);
}