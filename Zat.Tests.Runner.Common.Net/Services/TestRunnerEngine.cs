namespace Zat.Tests.Runner.Common.Net.Services;

using System.Collections.Generic;

using DevKit.Core.Extensions.Functional;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net;
using Zat.Tests.Runner.Common.Services;
using Zat.Z2xxTests.Common;
using Zat.Z2xxTests.Common.Model;

public class TestRunnerEngine(
    ITestRunnerBridgeConnector testRunnerBridgeConnector,
    INUnitTestRunnerProxy nunitTestRunnerProxy,
    IEnumerable<ITestResultHandler> testResultHandlers)
    : ITestRunnerEngine
{
    private IEnumerable<ITestResultHandler> testResultHandlers = testResultHandlers;

    private CancellationTokenSource? runTestCts;

    /// <inheritdoc />
    public bool IsRunning { get; set; }

    /// <inheritdoc />
    public async Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        string? testedRuntimeVersion,
        TestedHwAssemblyType[]? testedHwAssemblyTypes,
        bool isDebug,
        IEnumerable<ITestResultHandler>? resultHandlers = null)
    {
        var testRunResults = new List<TestResult>();

        ITestResultHandler[] finalTestResultHandlers =
        [
            ..resultHandlers is not null
                ? this.testResultHandlers.Concat(resultHandlers)
                : this.testResultHandlers
        ];

        var testEntitiesGroupedByType = testRunEntities
            .GroupBy(x => x.TestType)
            .ToArray();

        this.IsRunning = true;

        // Application tests execution
        var applicationTestEntities = testEntitiesGroupedByType
            .FirstOrDefault(x => x.Key is TestType.ApplicationTest)?
            .ToArray();
        if (applicationTestEntities is not null)
        {
            (await this.RunTestsAsync(
                    applicationTestEntities,
                    new TestConfig(
                        testedRuntimeVersion,
                        null,
                        isDebug),
                    finalTestResultHandlers))
                .Visit(testRunResults.Add);
        }

        // Runtime tests execution
        var runtimeTestEntities = testEntitiesGroupedByType
            .FirstOrDefault(x => x.Key is TestType.RuntimeTest)?
            .ToArray();
        if (runtimeTestEntities is not null)
        {
            ArgumentNullException.ThrowIfNull(testedHwAssemblyTypes);

            foreach (var testedHwAssemblyType in testedHwAssemblyTypes)
            {
                (await this.RunTestsAsync(
                        runtimeTestEntities,
                        new TestConfig(
                            testedRuntimeVersion,
                            testedHwAssemblyType,
                            isDebug),
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

    /// <inheritdoc />
    public void RegisterTestResultHandler(ITestResultHandler handler)
        => this.testResultHandlers = this.testResultHandlers.Append(handler);

    private async Task<TestResult> RunTestsAsync(
        IEnumerable<TestEntity> testRunEntities,
        TestConfig testConfig,
        ITestResultHandler[] resultHandlers)
    {
        using var connection = testRunnerBridgeConnector.Connect(testConfig);

        this.runTestCts = new CancellationTokenSource();

        var testRunResult =
            (await nunitTestRunnerProxy.RunTestAsync(testRunEntities))
            .Pipe(x => new TestResult(x, testConfig.TestedHwAssemblyType));

        foreach (var handler in resultHandlers)
        {
            handler.Handle(testRunResult);
        }

        return testRunResult;
    }
}