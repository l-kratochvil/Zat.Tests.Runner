namespace Zat.Tests.Runner.Common.Net.Services;

using System.Collections.Generic;

using Zat.Tests.Runner.Common;
using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;
using Zat.Z2xxTests.Common;
using Zat.Z2xxTests.Common.Model;

public class TestRunnerEngine(
    ITestRunnerBridgeConnector testRunnerBridgeConnector,
    INUnitTestRunnerProxy nunitTestRunnerProxy,
    IEnumerable<ITestResultHandler> testResultHandlers)
    : ITestRunnerEngine
{
    private CancellationTokenSource? runTestCts;

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public async Task<TestRunResult> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        string? testedRuntimeVersion,
        TestedHwAssemblyType[]? testedHwAssemblyTypes,
        bool isDebug,
        IEnumerable<ITestResultHandler>? resultHandlers = null)
    {
        using var connection = testRunnerBridgeConnector.Connect(testConfig);

        var finalTestResultHandlers = resultHandlers is not null
            ? testResultHandlers.Concat(resultHandlers)
            : testResultHandlers;

        this.runTestCts = new CancellationTokenSource();
        this.IsRunning = true;

        var testRunResult = await nunitTestRunnerProxy.RunTestAsync(
            testRunEntities);

        this.IsRunning = false;

        foreach (var handler in finalTestResultHandlers)
        {
            handler.Handle(testRunResult);
        }

        return testRunResult;
    }

    /// <inheritdoc />
    public void StopTestRun()
        => this.runTestCts?.Cancel();

    /// <inheritdoc />
    public void RegisterTestResultHandler(ITestResultHandler handler)
        => testResultHandlers = testResultHandlers.Append(handler);
}