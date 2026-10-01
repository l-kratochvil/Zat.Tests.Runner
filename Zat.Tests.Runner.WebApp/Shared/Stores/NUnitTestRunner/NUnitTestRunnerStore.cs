namespace Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;

using Microsoft.Extensions.Hosting;
using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.WebApp.Application.Paths;

/// <summary>
/// Store of test suites discovered from the test machine when the application starts.
/// </summary>
/// <remarks>
/// Discovery runs during startup, so <see cref="LoadedTestSuites"/> is ready before the first
/// browser connects.
/// </remarks>
/// <param name="proxy">Proxy the test assembly is discovered through.</param>
/// <param name="logger">Creates the log discovery failures are reported to.</param>
public sealed class NUnitTestRunnerStore(
    INUnitTestRunnerProxy proxy,
    ILogger<LogSources.TestRun> logger,
    IAppPathsProvider appPathsProvider)
    : INUnitTestRunnerStore, IHostedService
{
    // private const string TestAssemblyPath =
    //  @"c:\Users\l-kratochvil\source\repos\Zat.Tests.Runner\Tests\NUnitTestAssembly.Net481\bin\Debug\net481\NUnitTestAssembly.Net481.dll";

    /// <inheritdoc/>
    public IReadOnlyList<TestSuiteEntity> LoadedTestSuites { get; private set; } = [];

    /// <summary>
    /// Discovers test suites from the configured test assembly.
    /// </summary>
    /// <remarks>
    /// Discovery failures do not stop the application. The store stays empty and the log explains
    /// why.
    /// </remarks>
    /// <param name="cancellationToken">Token abandoning the start.</param>
    /// <returns>A task that completes after discovery has been attempted.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // var testAssemblyPath =
            //    @"c:\Users\l-kratochvil\source\repos\Zat.Tests.Runner\Tests\NUnitTestAssembly.Net481\bin\Debug\net481\NUnitTestAssembly.Net481.dll";
            var testAssemblyPath = appPathsProvider.Files.MainAssemblyDll;

            this.LoadedTestSuites = await proxy.LoadTestAssemblyAsync(testAssemblyPath, cancellationToken);

            logger.Log(
                LogLevel.Information,
                "Loaded {LoadedTestSuitesCount} test suites from {TestAssemblyPath}.",
                this.LoadedTestSuites.Count,
                testAssemblyPath);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Log(
                LogLevel.Warning,
                "The test assembly could not be read, so there are no tests to choose from.\nException: {Exception}",
                exception.ToString());
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}