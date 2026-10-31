namespace NUnitTestAssembly.Net481;

using System.IO.Pipes;

using StreamJsonRpc;

using Zat.Z2xxTests.Common.Services;

/// <summary>
/// The client end of the bridge to the test runner, a copy of the one in Zat.Z2xxTests.
/// </summary>
internal sealed class TestRunnerBridgeConnector : IDisposable
{
    // Shorter than in Zat.Z2xxTests, so test runs without a bridge don't wait long for it.
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(1);

    private readonly NamedPipeClientStream pipe;
    private readonly JsonRpc rpc;

    private TestRunnerBridgeConnector(ITestRunnerBridge bridge, NamedPipeClientStream pipe, JsonRpc rpc)
    {
        this.Bridge = bridge;
        this.pipe = pipe;
        this.rpc = rpc;
    }

    public ITestRunnerBridge Bridge { get; }

    public static async Task<TestRunnerBridgeConnector> ConnectAsync(CancellationToken cancellationToken = default)
    {
        NamedPipeClientStream? pipe = null;
        JsonRpc? rpc = null;

        try
        {
            pipe = new NamedPipeClientStream(
                ".", TestRunnerBridgeConfig.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipe
                .ConnectAsync((int)ConnectTimeout.TotalMilliseconds, cancellationToken)
                .ConfigureAwait(false);

            var handler = new HeaderDelimitedMessageHandler(pipe, pipe, new SystemTextJsonFormatter());

            rpc = new JsonRpc(handler);
            var bridge = rpc.Attach<ITestRunnerBridge>();
            rpc.StartListening();

            return new TestRunnerBridgeConnector(bridge, pipe, rpc);
        }
        catch
        {
            rpc?.Dispose();
            pipe?.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        this.rpc.Dispose();
        this.pipe.Dispose();
    }
}