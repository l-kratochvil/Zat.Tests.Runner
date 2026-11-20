namespace Zat.Tests.Runner.Common.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StreamJsonRpc;
using Zat.Tests.Runner.Common.Model;

/// <summary>
/// Surface of the out-of-process NUnit test runner. Implemented by the .NET Framework
/// proxy server and consumed by the application over a StreamJsonRpc named-pipe connection.
/// </summary>
[JsonRpcContract]
public interface INUnitTestRunnerProxy
{
    /// <summary>Discovers the tests from the assembly as test tree.</summary>
    Task<TestSuiteEntity[]> DiscoverTestsAsync(
        string testAssemblyDllPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs tests for the supplied test entities from the given test assembly.
    /// Cancelling <paramref name="cancellationToken"/> forcibly aborts the run.
    /// </summary>
    Task<ProxyTestResult> RunTestsAsync(
        string testAssemblyDllPath,
        IEnumerable<TestEntity> testRunEntities,
        CancellationToken cancellationToken = default);
}