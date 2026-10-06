namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Services;

/// <summary>
/// A connection to a running NUnit proxy server. Disposing it stops the server process together with every
/// process the tests started, so nothing it was running outlives it.
/// </summary>
public interface INUnitTestRunnerProxyConnection : IAsyncDisposable
{
    /// <summary>The remote NUnit test runner.</summary>
    INUnitTestRunnerProxy Proxy { get; }
}