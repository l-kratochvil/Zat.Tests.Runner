namespace Zat.Tests.Runner.Common.Net.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using StreamJsonRpc;

using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Owns the out-of-process NUnit proxy server: launches it, establishes a StreamJsonRpc connection over a
/// named pipe and exposes a strongly-typed <see cref="INUnitTestRunnerProxy"/>. Disposing tears the connection
/// down and stops the server process.
/// </summary>
public sealed class NUnitTestRunnerProxyConnector : IAsyncDisposable
{
    // The server and its .NET Framework dependencies are copied here by the build (Exchange output).
    private const string ServerRelativePath = @"Zat.Tests.Runner.NUnitTestRunnerProxy\Zat.Tests.Runner.NUnitTestRunnerProxy.exe";

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(60);

    private readonly Process serverProcess;
    private readonly NamedPipeServerStream pipe;
    private readonly JsonRpc rpc;

    /// <summary>The remote NUnit test runner.</summary>
    public INUnitTestRunnerProxy Proxy { get; }

    private NUnitTestRunnerProxyConnector(
        Process serverProcess,
        NamedPipeServerStream pipe,
        JsonRpc rpc,
        INUnitTestRunnerProxy proxy)
    {
        this.serverProcess = serverProcess;
        this.pipe = pipe;
        this.rpc = rpc;
        this.Proxy = proxy;
    }

    /// <summary>Launches the proxy server and connects to it.</summary>
    public static async Task<NUnitTestRunnerProxyConnector> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var serverPath = Path.Combine(AppContext.BaseDirectory, ServerRelativePath);
        if (!File.Exists(serverPath))
        {
            throw new FileNotFoundException($"NUnit proxy server executable was not found: {serverPath}", serverPath);
        }

        var pipeName = $"Zat.Tests.RunnerProxy_{Guid.NewGuid():N}";
        var pipe = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        Process? serverProcess = null;
        try
        {
            serverProcess = Process.Start(new ProcessStartInfo(serverPath, pipeName)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(serverPath)!,
            }) ?? throw new InvalidOperationException("Failed to start the NUnit proxy server process.");

            using var connectTimeout = new CancellationTokenSource(ConnectTimeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connectTimeout.Token);
            await pipe.WaitForConnectionAsync(linked.Token).ConfigureAwait(false);

            var formatter = new SystemTextJsonFormatter();
            var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

            var rpc = new JsonRpc(handler);
            var proxy = rpc.Attach<INUnitTestRunnerProxy>();
            rpc.StartListening();

            return new NUnitTestRunnerProxyConnector(serverProcess, pipe, rpc, proxy);
        }
        catch
        {
            await pipe.DisposeAsync().ConfigureAwait(false);
            KillProcess(serverProcess);
            throw;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Closing the RPC connection lets the server observe the disconnect and exit gracefully.
        try { this.rpc.Dispose(); }
        catch
        {
            /* best effort */
        }

        try { await this.pipe.DisposeAsync().ConfigureAwait(false); }
        catch
        {
            /* best effort */
        }

        KillProcess(this.serverProcess);
        this.serverProcess.Dispose();
    }

    private static void KillProcess(Process? process)
    {
        try
        {
            if (process is { HasExited: false })
            {
                process.Kill();
            }
        }
        catch
        {
            // Ignore cleanup failures.
        }
    }
}