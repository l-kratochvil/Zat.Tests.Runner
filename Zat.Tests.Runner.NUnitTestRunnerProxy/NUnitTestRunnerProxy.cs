namespace Zat.Tests.Runner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Out-of-process NUnit test runner exposed to the application over StreamJsonRpc. Each discovery and each run uses its
/// own <see cref="TestAssemblyDomain"/>; in production each call also runs in a dedicated proxy process that is
/// terminated once finished.
/// </summary>
public sealed class NUnitTestRunnerProxy : INUnitTestRunnerProxy
{
    /// <inheritdoc/>
    public Task<TestSuiteEntity[]> DiscoverTestsAsync(
        string testAssemblyDllPath, CancellationToken cancellationToken = default)
        => Task.Run(
            () =>
            {
                using var testAssemblyDomain = TestAssemblyDomain.Create(testAssemblyDllPath);
                return testAssemblyDomain.Runner.Load(testAssemblyDllPath);
            },
            cancellationToken);

    /// <inheritdoc/>
    public Task<ProxyTestResult> RunTestsAsync(
        string testAssemblyDllPath,
        IEnumerable<TestEntity> testRunEntities,
        CancellationToken cancellationToken = default)
    {
        string[] executionPaths = [.. testRunEntities.Select(x => x.ExecutionPath)];

        return Task.Run(
            () =>
            {
                using var testAssemblyDomain = TestAssemblyDomain.Create(testAssemblyDllPath);
                var runner = testAssemblyDomain.Runner;

                cancellationToken.ThrowIfCancellationRequested();
                runner.Load(testAssemblyDllPath);

                // Cancellation forcibly aborts the in-progress run; disposed before the AppDomain unloads.
                using var cancellationRegistration = cancellationToken.Register(runner.StopRun);
                cancellationToken.ThrowIfCancellationRequested();

                return runner.Run(executionPaths);
            },
            cancellationToken);
    }
}