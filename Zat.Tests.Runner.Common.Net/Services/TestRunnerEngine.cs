namespace Zat.Tests.Runner.Common.Net.Services;

using System.Collections.Generic;

using Zat.Tests.Runner.Common;
using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;
using Zat.Z2xxTests.Common.Model;

public class TestRunnerEngine(
    ITestRunnerBridgeConnector testRunnerBridgeConnector,
    INUnitTestRunnerProxy nunitTestRunnerProxy)
    : ITestRunnerEngine
{
    private CancellationTokenSource? runTestCts;

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public async Task<TestRunResult> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        TestConfig testConfig)
    {
        using var connection = testRunnerBridgeConnector.Connect(testConfig);

        this.runTestCts = new CancellationTokenSource();
        this.IsRunning = true;

        var testRunResult = await nunitTestRunnerProxy.RunTestAsync(
            testRunEntities);

        this.IsRunning = false;

        return testRunResult;
    }

    /// <inheritdoc />
    public void StopTestRun()
        => this.runTestCts?.Cancel();
}