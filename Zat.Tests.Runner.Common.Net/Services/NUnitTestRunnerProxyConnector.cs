namespace Zat.Tests.Runner.Common.Net.Services;

using System.Diagnostics;
using System.IO.Pipes;

using DevKit.Core.Interfaces;

using StreamJsonRpc;

using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Launches the out-of-process NUnit proxy server on demand and connects to it over a StreamJsonRpc named pipe.
/// </summary>
/// <remarks>
/// Initialising checks only that the server is installed, so a broken installation stops the application at
/// start-up rather than at the first test run, without the cost of starting the server.
/// </remarks>
/// <param name="launchDebugger">When <see langword="true"/>, passes <c>--debug</c> to the server so a Debug build
/// asks for a debugger on start-up.</param>
public sealed class NUnitTestRunnerProxyConnector(bool launchDebugger)
    : INUnitTestRunnerProxyConnector, IInitializable
{
    // The server and its .NET Framework dependencies are copied here by the build (Exchange output).
    private const string ServerRelativePath = @"Zat.Tests.Runner.NUnitTestRunnerProxy\Zat.Tests.Runner.NUnitTestRunnerProxy.exe";

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(60);

    private static string ServerPath => Path.Combine(AppContext.BaseDirectory, ServerRelativePath);

    /// <inheritdoc/>
    /// <exception cref="FileNotFoundException">The proxy server executable is missing.</exception>
    public void Initialize()
        => EnsureServerExists();

    /// <inheritdoc/>
    public async Task<INUnitTestRunnerProxyConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        EnsureServerExists();

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
            var arguments = launchDebugger
                ? $"{pipeName} {CommonConstants.ProcessArgs.Debug}"
                : pipeName;

            serverProcess = Process.Start(new ProcessStartInfo(ServerPath, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(ServerPath)!,
            }) ?? throw new InvalidOperationException("Failed to start the NUnit proxy server process.");

            using var connectTimeout = new CancellationTokenSource(ConnectTimeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connectTimeout.Token);
            await pipe.WaitForConnectionAsync(linked.Token).ConfigureAwait(false);

            var formatter = new SystemTextJsonFormatter();
            var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

            var rpc = new JsonRpc(handler);
            var proxy = rpc.Attach<INUnitTestRunnerProxy>();
            rpc.StartListening();

            return new NUnitTestRunnerProxyConnection(serverProcess, pipe, rpc, proxy);
        }
        catch
        {
            await pipe.DisposeAsync().ConfigureAwait(false);
            await NUnitTestRunnerProxyConnection.KillAsync(serverProcess).ConfigureAwait(false);
            serverProcess?.Dispose();
            throw;
        }
    }

    private static void EnsureServerExists()
    {
        if (!File.Exists(ServerPath))
        {
            throw new FileNotFoundException($"NUnit proxy server executable was not found: {ServerPath}", ServerPath);
        }
    }
}