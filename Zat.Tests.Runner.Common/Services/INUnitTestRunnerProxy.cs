namespace Zat.Tests.Runner.Common.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// Surface of the out-of-process NUnit test runner. Implemented by the .NET Framework
/// proxy server and consumed by the application over a StreamJsonRpc named-pipe connection.
/// </summary>
public interface INUnitTestRunnerProxy
{
    /// <summary>Loads the test assembly and returns its discovered test tree.</summary>
    Task<TestSuiteEntity[]> LoadTestAssemblyAsync(
        string assemblyDllPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the supplied test entities. Cancelling <paramref name="cancellationToken"/> forcibly aborts the run.
    /// </summary>
    Task<ProxyTestResult> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities, CancellationToken cancellationToken = default);

    /// <summary>Indicates whether a test assembly is currently loaded.</summary>
    Task<bool> GetIsAssemblyLoadedAsync(CancellationToken cancellationToken = default);

    /// <summary>Indicates whether a test run is currently in progress.</summary>
    Task<bool> GetIsTestRunningAsync(CancellationToken cancellationToken = default);
}