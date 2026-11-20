namespace Zat.Tests.Runner.Common.Net.Services;

using System.IO.Pipes;

using DevKit.Core.UtilityObjects;

using StreamJsonRpc;

using Zat.Z2xxTests.Common.Model;
using Zat.Z2xxTests.Common.Services;

/// <summary>Opens the bridge pipe and starts listening for an optional client connection.</summary>
public class TestRunnerBridgeConnector : ITestRunnerBridgeConnector
{
    /// <inheritdoc/>
    public IAsyncDisposable Connect(TestConfig testConfig)
    {
        // Constructing the stream reserves the pipe name, so a client starting right after this
        // call returns already finds the pipe, even before the connection is accepted below.
        var pipe = new NamedPipeServerStream(
            TestRunnerBridgeConfig.PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        var formatter = new SystemTextJsonFormatter();
        var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

        var rpc = new JsonRpc(handler);
        rpc.AddLocalRpcTarget<ITestRunnerBridge>(
            new TestRunnerBridge(testConfig),
            new JsonRpcTargetOptions { DisposeOnDisconnect = true });

        var connectionWaitingCts = new CancellationTokenSource();
        var listeningTask = ListenOnceConnectedAsync(pipe, rpc, connectionWaitingCts.Token);

        return new AsyncDisposer(async () =>
        {
            // Settle the wait first, so rpc can't start listening on a pipe that is being disposed.
            await connectionWaitingCts.CancelAsync();
            await listeningTask.ConfigureAwait(false);
            connectionWaitingCts.Dispose();

            // Dispose rpc first so it goes through its clean LocallyDisposed shutdown path,
            // which then disposes the pipe itself; disposing the pipe first would surface as
            // a StreamError instead of a graceful disconnect.
            rpc.Dispose();
            await pipe.DisposeAsync();
        });
    }

    /// <remarks>
    /// Reading the pipe before a client connects fails, which would make rpc drop the pipe and leave the
    /// client to time out.
    /// </remarks>
    private static async Task ListenOnceConnectedAsync(
        NamedPipeServerStream pipe,
        JsonRpc rpc,
        CancellationToken ct)
    {
        try
        {
            await pipe
                .WaitForConnectionAsync(ct)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        rpc.StartListening();
    }
}