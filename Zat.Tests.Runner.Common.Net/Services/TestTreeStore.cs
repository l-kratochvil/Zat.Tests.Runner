namespace Zat.Tests.Runner.Common.Net.Services;

using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <inheritdoc cref="ITestTreeStore"/>
/// <param name="proxyConnector">Connects to the proxy the test assembly is discovered through.</param>
/// <param name="directoryWatcher">Tells when the directory of the test assembly changes.</param>
/// <param name="timeProvider">Measures the quiet period after a change.</param>
/// <param name="appPathsProvider">Tells where the test assembly is.</param>
/// <param name="logger">Log the discoveries are reported to.</param>
public sealed class TestTreeStore(
    INUnitTestRunnerProxyConnector proxyConnector,
    IDirectoryWatcher directoryWatcher,
    TimeProvider timeProvider,
    ISharedAppPathsProvider appPathsProvider,
    ILogger<LogSources.TestRun> logger)
    : ITestTreeStore, IDisposable
{
    /// <summary>
    /// How long the directory of the test assembly has to stay unchanged before the test tree is discovered
    /// again. Deploying the test assembly writes many files, and the test tree is read once they are all there.
    /// </summary>
    internal static readonly TimeSpan QuietPeriod = TimeSpan.FromSeconds(2);

    // One discovery at a time, so a slow one can't overwrite the test tree of a later one.
    private readonly SemaphoreSlim discoveryLock = new(initialCount: 1, maxCount: 1);

    // Guards the watching, which the watcher's threads and Dispose touch at once.
    private readonly Lock watchingLock = new();

    private IDisposable? watching;
    private ITimer? quietPeriodTimer;
    private bool disposed;

    /// <inheritdoc/>
    public event Action<TestTreeChange>? Changed;

    /// <inheritdoc/>
    public IReadOnlyList<TestSuiteEntity> TestSuites { get; private set; } = [];

    private string TestAssemblyDllPath
        => appPathsProvider.Files.MainAssemblyDll;

    /// <inheritdoc/>
    public async Task DiscoverAsync(CancellationToken cancellationToken = default)
    {
        this.StartWatching();

        await this.discoveryLock.WaitAsync(cancellationToken);
        try
        {
            this.Replace(await this.ReadTestAssemblyAsync(cancellationToken));
        }
        finally
        {
            this.discoveryLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (this.watchingLock)
        {
            this.disposed = true;
            this.watching?.Dispose();
            this.quietPeriodTimer?.Dispose();
        }
    }

    private void StartWatching()
    {
        lock (this.watchingLock)
        {
            if (this.watching is not null || this.disposed)
            {
                return;
            }

            this.quietPeriodTimer = timeProvider.CreateTimer(
                _ => _ = this.DiscoverAgainAsync(),
                state: null,
                dueTime: Timeout.InfiniteTimeSpan,
                period: Timeout.InfiniteTimeSpan);
            this.watching = directoryWatcher.Watch(
                Path.GetDirectoryName(this.TestAssemblyDllPath) ?? this.TestAssemblyDllPath,
                this.OnTestAssemblyChanged);
        }
    }

    private void OnTestAssemblyChanged()
    {
        lock (this.watchingLock)
        {
            // Every change starts the quiet period anew.
            this.quietPeriodTimer?.Change(dueTime: QuietPeriod, period: Timeout.InfiniteTimeSpan);
        }
    }

    private async Task DiscoverAgainAsync()
    {
        await this.discoveryLock.WaitAsync();
        try
        {
            IReadOnlyList<TestSuiteEntity> testSuites;
            try
            {
                testSuites = await this.ReadTestAssemblyAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.Log(
                    LogLevel.Warning,
                    "The test assembly {TestAssemblyPath} changed but could not be read, so there are no tests to choose from until it changes again.\nException: {Exception}",
                    this.TestAssemblyDllPath,
                    exception.ToString());
                testSuites = [];
            }

            if (this.Replace(testSuites))
            {
                logger.Log(
                    LogLevel.Information,
                    "The test assembly {TestAssemblyPath} changed, so the tests to choose from were read again and the test selection was cleared.",
                    this.TestAssemblyDllPath);
            }
        }
        finally
        {
            this.discoveryLock.Release();
        }
    }

    private async Task<IReadOnlyList<TestSuiteEntity>> ReadTestAssemblyAsync(CancellationToken cancellationToken)
    {
        await using var connection = await proxyConnector.ConnectAsync(cancellationToken);
        var testSuites = await connection.Proxy.LoadTestAssemblyAsync(this.TestAssemblyDllPath, cancellationToken);

        logger.Log(
            LogLevel.Information,
            "Loaded {LoadedTestSuitesCount} test suites from {TestAssemblyPath}.",
            testSuites.Length,
            this.TestAssemblyDllPath);

        return testSuites;
    }

    /// <returns>Whether the execution paths of the test tree changed.</returns>
    private bool Replace(IReadOnlyList<TestSuiteEntity> testSuites)
    {
        var executionPathsChanged = !ExecutionPaths(this.TestSuites).SetEquals(ExecutionPaths(testSuites));

        this.TestSuites = testSuites;
        this.Changed?.Invoke(new TestTreeChange(testSuites, executionPathsChanged));

        return executionPathsChanged;
    }

    private static HashSet<string> ExecutionPaths(IEnumerable<TestSuiteEntity> testSuites)
        => testSuites
            .AllTestEntities()
            .Select(testEntity => testEntity.ExecutionPath)
            .ToHashSet(StringComparer.Ordinal);
}
