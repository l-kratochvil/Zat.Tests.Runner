namespace Zat.Tests.Runner.Common.Net.Services;

using System.Collections.Generic;

using DevKit.Core.Extensions.Functional;

using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Properties;
using Zat.Tests.Runner.Common.Services;
using Zat.Z2xxTests.Common.Model;

public class TestRunnerEngine(
    ITestRunnerBridgeConnector testRunnerBridgeConnector,
    INUnitTestRunnerProxy nunitTestRunnerProxy,
    ILogger<LogSources.TestRun> logger)
    : ITestRunnerEngine
{
    // Every change of State happens under this lock, so a check and the change it allows can't interleave with
    // another one, and StateChanged reports the changes in the order they happened.
    private readonly Lock stateLock = new();

    private CancellationTokenSource? runTestCts;

    /// <inheritdoc />
    public event Action<TestRunState>? StateChanged;

    /// <inheritdoc />
    public TestRunState State
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }
            
            field = value;

            logger.Log(
                LogLevel.Information,
                value switch
                {
                    TestRunState.Running => Resources.TestRun_Started,
                    TestRunState.Stopping => Resources.TestRun_Stopping,
                    TestRunState.Stopped => Resources.TestRun_Stopped,
                    _ => throw new ArgumentOutOfRangeException(nameof(value)),
                });

            this.StateChanged?.Invoke(value);
        }
    }

    /// <inheritdoc />
    public async Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        ITestRunnerEngine.Config config,
        CancellationToken cancellationToken = default)
    {
        var subRuns = PlanSubRuns(testRunEntities, config);
        ITestResultHandler[] resultHandlers = [.. config.TestResultHandlers];

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var stopRequestedRegistration = default(CancellationTokenRegistration);
        var started = false;
        try
        {
            lock (this.stateLock)
            {
                if (this.State is not TestRunState.Stopped)
                {
                    throw new InvalidOperationException("A test run is already in progress.");
                }

                // Set before the state changes, so even a listener throwing on Running can't leave the engine
                // stuck in it.
                started = true;
                this.runTestCts = runCts;
                this.State = TestRunState.Running;
            }

            // Both the caller's token and StopTestRun end up cancelling runCts.
            stopRequestedRegistration = runCts.Token.Register(this.OnStopRequested);

            var testRunResults = new List<TestResult>();

            foreach (var subRun in subRuns.TakeWhile(_ => !runCts.IsCancellationRequested))
            {
                (await this.RunSubRunAsync(subRun, resultHandlers, runCts.Token))
                    .VisitIfNotNull(testRunResults.Add);
            }

            return [.. testRunResults];
        }
        finally
        {
            await stopRequestedRegistration.DisposeAsync();

            if (started)
            {
                lock (this.stateLock)
                {
                    this.runTestCts = null;
                    this.State = TestRunState.Stopped;
                }
            }
        }
    }

    /// <inheritdoc />
    public void StopTestRun()
    {
        lock (this.stateLock)
        {
            this.runTestCts?.Cancel();
        }
    }

    /// <summary>
    /// Splits <paramref name="testRunEntities"/> into the runs the proxy executes one by one: the application tests
    /// once, the runtime tests once per tested HW assembly type.
    /// </summary>
    private static SubRun[] PlanSubRuns(IEnumerable<TestEntity> testRunEntities, ITestRunnerEngine.Config config)
    {
        var testEntitiesByType = testRunEntities.ToLookup(x => x.TestType);
        var applicationTestEntities = testEntitiesByType[TestType.Application].ToArray();
        var runtimeTestEntities = testEntitiesByType[TestType.Runtime].ToArray();

        var subRuns = new List<SubRun>();

        if (applicationTestEntities.Length > 0)
        {
            subRuns.Add(new SubRun(
                applicationTestEntities,
                new TestConfig(config.TestedRuntimeVersion, null, config.IsDebug),
                TestType.Application));
        }

        if (runtimeTestEntities.Length > 0)
        {
            ArgumentNullException.ThrowIfNull(config.TestedHwAssemblyTypes);

            subRuns.AddRange(config.TestedHwAssemblyTypes.Select(testedHwAssemblyType => new SubRun(
                runtimeTestEntities,
                new TestConfig(config.TestedRuntimeVersion, testedHwAssemblyType, config.IsDebug),
                TestType.Runtime)));
        }

        return [.. subRuns];
    }

    private void OnStopRequested()
    {
        lock (this.stateLock)
        {
            if (this.State is TestRunState.Running)
            {
                this.State = TestRunState.Stopping;
            }
        }
    }

    /// <returns>The test result of <paramref name="subRun"/>, or <see langword="null"/> when it was stopped before
    /// the proxy got to any of its test cases.</returns>
    private async Task<TestResult?> RunSubRunAsync(
        SubRun subRun,
        ITestResultHandler[] resultHandlers,
        CancellationToken cancellationToken)
    {
        using var connection = testRunnerBridgeConnector.Connect(subRun.TestConfig);

        ProxyTestResult proxyTestResult;
        try
        {
            proxyTestResult = await nunitTestRunnerProxy.RunTestAsync(subRun.TestEntities, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var testRunResult = new TestResult(proxyTestResult, subRun.TestType, subRun.TestConfig.TestedHwAssemblyType);

        // A sub-run cut short by a stop holds test cases that never ran, so it is not reported as their outcome.
        if (!cancellationToken.IsCancellationRequested)
        {
            foreach (var handler in resultHandlers)
            {
                handler.Handle(testRunResult);
            }
        }

        return testRunResult;
    }

    private sealed record SubRun(TestEntity[] TestEntities, TestConfig TestConfig, TestType TestType);
}