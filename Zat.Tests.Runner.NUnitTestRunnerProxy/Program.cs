using System.Diagnostics;
using System.IO.Pipes;

using DevKit.Core.Utils;

using StreamJsonRpc;
using Zat.Tests.Runner.Common;
using Zat.Tests.Runner.NUnitTestRunnerProxy;

#if DEBUG
var debugRequested = args.Skip(1).Any(
    arg => string.Equals(
        arg, CommonConstants.ProcessArgs.Debug, StringComparison.OrdinalIgnoreCase));
if (debugRequested && DebuggerUtils.NetDebuggerLaunchAllowed)
{
    Debugger.Launch();
}
#endif

if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
{
    await Console.Error.WriteLineAsync("Usage: Zat.Tests.Runner.NUnitTestRunnerProxy.Server <pipe-name> [--debug]");
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

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
    await rpc.Completion.ConfigureAwait(false);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
    return 0;
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Server failed: {ex}");
    return 1;
}