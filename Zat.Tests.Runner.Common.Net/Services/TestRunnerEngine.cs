#pragma warning disable CA2254
namespace Zat.Tests.Runner.Common.Net.Services;

using System.Collections.Generic;
using System.Diagnostics;

using CSharpFunctionalExtensions;

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
    INUnitTestRunnerProxyConnector nunitTestRunnerProxyConnector,
    ILogger<LogSources.TestRun> logger,
    ILogger<LogSources.App> appLogger)
    : ITestRunnerEngine
{
    /// <summary>
    /// How long a stopped test run waits for the proxy to give in. Then the engine stops waiting and ends the
    /// connection, which kills the proxy server along with whatever the test cases started.
    /// </summary>
    internal static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(5);

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
                    TestRunState.Running => "Test run started",
                    TestRunState.Stopping => "Test run stopping",
                    TestRunState.Stopped => "Test run stopped",
                    _ => throw new ArgumentOutOfRangeException(nameof(value)),
                });

            appLogger.Log(
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
    public async Task<Result<TestResult[]>> RunTestAsync(
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

            var stopwatch = Stopwatch.StartNew();
            var testRunResults = new List<TestResult>();

            try
            {
                // The whole test run gets a proxy server of its own, so ending the connection is sure to end the
                // test run as well.
                await using var connection = await nunitTestRunnerProxyConnector.ConnectAsync(runCts.Token);
                var testTree = await WaitUnlessAbandonedAsync(
                    connection.Proxy.LoadTestAssemblyAsync(config.TestAssemblyDllPath, runCts.Token),
                    runCts.Token);

                // The test assembly may have changed since the test run entities were discovered from it. NUnit
                // would quietly skip whatever is gone, so the test run doesn't start at all.
                var missingExecutionPaths = FindMissingExecutionPaths(subRuns, testTree);
                if (missingExecutionPaths.Length > 0)
                {
                    logger.Log(
                        LogLevel.Error,
                        "The test run did not start: the test assembly {TestAssemblyPath} changed and no longer holds these selected tests: {MissingExecutionPaths}",
                        config.TestAssemblyDllPath,
                        missingExecutionPaths);

                    var error = string.Format(
                        Resources.TestRun_TestEntitiesMissing_Format,
                        config.TestAssemblyDllPath,
                        string.Join(Environment.NewLine, missingExecutionPaths));

                    appLogger.Log(LogLevel.Error, error);

                    return Result.Failure<TestResult[]>(error);
                }

                foreach (var subRun in subRuns.TakeWhile(_ => !runCts.IsCancellationRequested))
                {
                    (await this.RunSubRunAsync(connection.Proxy, subRun, resultHandlers, runCts.Token))
                        .VisitIfNotNull(testRunResults.Add);
                }
            }
            catch (OperationCanceledException) when (runCts.IsCancellationRequested)
            {
                // Stopped before the proxy got to any test case.
            }

            var testRunResult = new TestRunResult(
                [.. testRunResults],
                IsStoppedByUser: runCts.IsCancellationRequested,
                ElapsedTime: stopwatch.Elapsed);
            foreach (var handler in resultHandlers)
            {
                handler.HandleTestRunResult(testRunResult);
            }

            return Result.Success<TestResult[]>([.. testRunResults]);
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

        // ReSharper disable once InvertIf
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

    /// <returns>The execution paths of the test entities <paramref name="subRuns"/> run that
    /// <paramref name="testTree"/> doesn't hold, in the order they were selected.</returns>
    private static string[] FindMissingExecutionPaths(SubRun[] subRuns, TestSuiteEntity[] testTree)
    {
        var existingExecutionPaths = testTree
            .AllTestEntities()
            .Select(testEntity => testEntity.ExecutionPath)
            .ToHashSet(StringComparer.Ordinal);

        return
        [
            .. subRuns
                .SelectMany(subRun => subRun.TestEntities)
                .Select(testEntity => testEntity.ExecutionPath)
                .Distinct(StringComparer.Ordinal)
                .Where(executionPath => !existingExecutionPaths.Contains(executionPath)),
        ];
    }

    /// <summary>
    /// Awaits <paramref name="task"/>, but no longer than <see cref="StopTimeout"/> after
    /// <paramref name="stopToken"/> was cancelled.
    /// </summary>
    /// <remarks>
    /// The proxy learns about the stop from the token it was handed. This only limits how long it gets to comply.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The proxy did not give in to the stop in time.</exception>
    private static async Task<T> WaitUnlessAbandonedAsync<T>(Task<T> task, CancellationToken stopToken)
    {
        using var abandonCts = new CancellationTokenSource();
        await using var stopRegistration = stopToken.Register(() => abandonCts.CancelAfter(StopTimeout));

        return await task.WaitAsync(abandonCts.Token);
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
        INUnitTestRunnerProxy proxy,
        SubRun subRun,
        ITestResultHandler[] resultHandlers,
        CancellationToken cancellationToken)
    {
        using var connection = testRunnerBridgeConnector.Connect(subRun.TestConfig);

        ProxyTestResult proxyTestResult;
        try
        {
            proxyTestResult = await WaitUnlessAbandonedAsync(
                proxy.RunTestAsync(subRun.TestEntities, cancellationToken),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var testRunResult = new TestResult(proxyTestResult, subRun.TestType, subRun.TestConfig.TestedHwAssemblyType);

        // A sub-run cut short by a stop holds test cases that never ran, so it is not reported as their outcome.
        if (cancellationToken.IsCancellationRequested)
        {
            return testRunResult;
        }

        foreach (var handler in resultHandlers)
        {
            handler.HandleSubRunResult(testRunResult);
        }

        return testRunResult;
    }

    private sealed record SubRun(TestEntity[] TestEntities, TestConfig TestConfig, TestType TestType);
}