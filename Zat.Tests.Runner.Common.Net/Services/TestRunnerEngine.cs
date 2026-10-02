namespace Zat.Tests.Runner.Common.Net.Services;

using System.Collections.Generic;

using DevKit.Core.Extensions.Functional;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Model;
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
    public async Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        ITestRunnerEngine.Config config)
    {
        var testRunResults = new List<TestResult>();

        ITestResultHandler[] finalTestResultHandlers = [.. config.TestResultHandlers];

        var testEntitiesGroupedByType = testRunEntities
            .GroupBy(x => x.TestType)
            .ToArray();

        this.IsRunning = true;

        // Application tests execution
        var applicationTestEntities = testEntitiesGroupedByType
            .FirstOrDefault(x => x.Key is TestType.Application)?
            .ToArray();
        if (applicationTestEntities is not null)
        {
            (await this.RunTestsAsync(
                    applicationTestEntities,
                    new TestConfig(
                        config.TestedRuntimeVersion,
                        null,
                        config.IsDebug),
                    TestType.Application,
                    finalTestResultHandlers))
                .Visit(testRunResults.Add);
        }

        // Runtime tests execution
        var runtimeTestEntities = testEntitiesGroupedByType
            .FirstOrDefault(x => x.Key is TestType.Runtime)?
            .ToArray();
        if (runtimeTestEntities is not null)
        {
            ArgumentNullException.ThrowIfNull(config.TestedHwAssemblyTypes);

            foreach (var testedHwAssemblyType in config.TestedHwAssemblyTypes)
            {
                (await this.RunTestsAsync(
                        runtimeTestEntities,
                        new TestConfig(
                            config.TestedRuntimeVersion,
                            testedHwAssemblyType,
                            config.IsDebug),
                        TestType.Runtime,
                        finalTestResultHandlers))
                    .Visit(testRunResults.Add);
            }
        }

        this.IsRunning = false;

        return [..testRunResults];
    }

    /// <inheritdoc />
    public void StopTestRun()
        => this.runTestCts?.Cancel();

    private async Task<TestResult> RunTestsAsync(
        IEnumerable<TestEntity> testRunEntities,
        TestConfig testConfig,
        TestType testType,
        ITestResultHandler[] resultHandlers)
    {
        using var connection = testRunnerBridgeConnector.Connect(testConfig);

        this.runTestCts = new CancellationTokenSource();

        var testRunResult =
            (await nunitTestRunnerProxy.RunTestAsync(testRunEntities))
            .Pipe(x => new TestResult(x, testType, testConfig.TestedHwAssemblyType));

        foreach (var handler in resultHandlers)
        {
            handler.Handle(testRunResult);
        }

        return testRunResult;
    }
}