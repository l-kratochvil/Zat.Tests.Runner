namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Starts the out-of-process NUnit proxy server on demand. Each connection owns a server process of its own,
/// so ending the connection ends whatever the server was doing.
/// </summary>
public interface INUnitTestRunnerProxyConnector
{
    /// <summary>Launches a proxy server and connects to it.</summary>
    /// <param name="cancellationToken">Cancels the connection attempt.</param>
    /// <returns>The connection; disposing it stops the server process.</returns>
    Task<INUnitTestRunnerProxyConnection> ConnectAsync(CancellationToken cancellationToken = default);
}