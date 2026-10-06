namespace Zat.Tests.Runner.WebApp.Shared.Services;

using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Discovers the test tree when the application starts, so it is ready before the first browser connects.
/// </summary>
/// <param name="testTreeStore">Store the test tree is discovered into.</param>
/// <param name="logger">Log a discovery failure is reported to.</param>
public sealed class TestTreeDiscovery(
    ITestTreeStore testTreeStore,
    ILogger<LogSources.TestRun> logger)
    : IHostedService
{
    /// <summary>
    /// Discovers the test tree from the configured test assembly.
    /// </summary>
    /// <remarks>
    /// A failure does not stop the application: there is nothing to choose from until the test assembly
    /// changes, and the log says why.
    /// </remarks>
    /// <param name="cancellationToken">Token abandoning the start.</param>
    /// <returns>A task that completes after discovery has been attempted.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await testTreeStore.DiscoverAsync(cancellationToken);
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