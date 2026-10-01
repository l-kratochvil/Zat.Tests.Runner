using System.Diagnostics;
using System.IO.Pipes;

using DevKit.Core.Utils;

using StreamJsonRpc;

using Zat.Tests.Runner.NUnitTestRunnerProxy;

if (DebuggerUtils.NetDebuggerLaunchAllowed)
{
    Debugger.Launch();
}

if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
{
    await Console.Error.WriteLineAsync("Usage: Zat.Tests.Runner.NUnitTestRunnerProxy.Server <pipe-name>");
    return 1;
}

var pipeName = args[0];

try
{
    using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipe.ConnectAsync((int)TimeSpan.FromSeconds(15).TotalMilliseconds).ConfigureAwait(false);

    var formatter = new SystemTextJsonFormatter();
    var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

    using var rpc = new JsonRpc(handler);
    rpc.AddLocalRpcTarget(new NUnitTestRunnerProxy(), new JsonRpcTargetOptions { DisposeOnDisconnect = true });
    rpc.StartListening();

    await rpc.Completion.ConfigureAwait(false);
    return 0;
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Server failed: {ex}");
    return 1;
}