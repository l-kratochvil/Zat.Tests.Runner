namespace Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;

using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Shared.Logging;

/// <summary>
/// Store of test suites discovered from the test machine when the application starts.
/// </summary>
/// <remarks>
/// Discovery runs during startup, so <see cref="LoadedTestSuites"/> is ready before the first
/// browser connects.
/// </remarks>
/// <param name="proxy">Proxy the test assembly is discovered through.</param>
/// <param name="loggerFactory">Creates the log discovery failures are reported to.</param>
public sealed class NUnitTestRunnerStore(
    INUnitTestRunnerProxy proxy,
    IAppLoggerFactory loggerFactory,
    IAppPathsProvider appPathsProvider)
    : INUnitTestRunnerStore, IHostedService
{
    // private const string TestAssemblyPath =
    //    @"c:\Users\l-kratochvil\source\repos\Zat.Tests.Runner\Tests\NUnitTestAssembly.Net481\bin\Debug\net481\NUnitTestAssembly.Net481.dll";

    private readonly IAppLogger logger = loggerFactory.CreateLogger(LogSources.TestRun);

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

            this.logger.Info($"Loaded {this.LoadedTestSuites.Count} test suites.", testAssemblyPath);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            this.logger.Warning(
                "The test assembly could not be read, so there are no tests to choose from.",
                exception.ToString());
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}