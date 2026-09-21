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
    public Disposer Connect(TestConfig testConfig)
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

        rpc.StartListening();

        return new Disposer(() =>
        {
            // Dispose rpc first so it goes through its clean LocallyDisposed shutdown path,
            // which then disposes the pipe itself; disposing the pipe first would surface as
            // a StreamError instead of a graceful disconnect.
            rpc.Dispose();
            pipe.Dispose();
        });
    }
}