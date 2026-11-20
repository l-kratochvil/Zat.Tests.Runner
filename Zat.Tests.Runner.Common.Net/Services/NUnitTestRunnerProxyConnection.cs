namespace Zat.Tests.Runner.Common.Net.Services;

using System.Diagnostics;
using System.IO.Pipes;

using StreamJsonRpc;

using Zat.Tests.Runner.Common.Services;

/// <inheritdoc cref="INUnitTestRunnerProxyConnection"/>
internal sealed class NUnitTestRunnerProxyConnection(
    Process serverProcess,
    NamedPipeServerStream pipe,
    JsonRpc rpc,
    INUnitTestRunnerProxy proxy)
    : INUnitTestRunnerProxyConnection
{
    private static readonly TimeSpan ExitTimeout = TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    public INUnitTestRunnerProxy Proxy { get; } = proxy;

    /// <summary>The id of the server process this connection owns.</summary>
    internal int ServerProcessId
        => serverProcess.Id;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Closing the RPC connection lets the server observe the disconnect and exit gracefully.
        try
        {
            rpc.Dispose();
        }
        catch
        {
            /* best effort */
        }

        try
        {
            await pipe.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            /* best effort */
        }

        await KillAsync(serverProcess).ConfigureAwait(false);
        serverProcess.Dispose();
    }

    /// <summary>
    /// Kills <paramref name="process"/> with all its descendants and waits for it to exit, so that nothing it
    /// started still holds the test station once this returns.
    /// </summary>
    internal static async Task KillAsync(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);

            using var exitTimeout = new CancellationTokenSource(ExitTimeout);
            await process.WaitForExitAsync(exitTimeout.Token).ConfigureAwait(false);
        }
        catch
        {
            // Ignore cleanup failures.
        }
    }
}