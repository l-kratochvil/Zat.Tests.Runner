namespace Zat.Tests.Runner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevKit.Core.Extensions;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Out-of-process NUnit test runner exposed to the application over StreamJsonRpc. Each loaded test assembly gets its
/// own <see cref="TestAssemblyDomain"/>, which replaces the one of the previously loaded test assembly.
/// </summary>
public sealed class NUnitTestRunnerProxy : INUnitTestRunnerProxy, IDisposable
{
    private TestAssemblyDomain? testAssemblyDomain;

    /// <inheritdoc/>
    public Task<bool> GetIsAssemblyLoadedAsync(CancellationToken cancellationToken = default)
        => (this.testAssemblyDomain?.Runner.IsTestLoaded ?? false).AsTask();

    /// <inheritdoc/>
    public Task<bool> GetIsTestRunningAsync(CancellationToken cancellationToken = default)
        => (this.testAssemblyDomain?.Runner.IsTestRunning ?? false).AsTask();

    /// <inheritdoc/>
    public Task<TestSuiteEntity[]> LoadTestAssemblyAsync(
        string assemblyDllPath, CancellationToken cancellationToken = default)
        => Task.Run(
            () =>
            {
                this.UnloadTestAssemblyDomain();
                this.testAssemblyDomain = TestAssemblyDomain.Create(assemblyDllPath);

                return this.testAssemblyDomain.Runner.Load(assemblyDllPath);
            },
            cancellationToken);

    /// <inheritdoc/>
    public Task<ProxyTestResult> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities, CancellationToken cancellationToken = default)
    {
        var runner = this.testAssemblyDomain?.Runner
            ?? throw new InvalidOperationException("Test assembly wasn't loaded yet");

        string[] executionPaths = [.. testRunEntities.Select(x => x.ExecutionPath)];

        // Cancellation forcibly aborts the in-progress run.
        var cancellationRegistration = cancellationToken.Register(runner.StopRun);

        return Task.Run(
            () =>
            {
                try
                {
                    return runner.Run(executionPaths);
                }
                finally
                {
                    cancellationRegistration.Dispose();
                }
            },
            cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
        => this.UnloadTestAssemblyDomain();

    private void UnloadTestAssemblyDomain()
    {
        this.testAssemblyDomain?.Dispose();
        this.testAssemblyDomain = null;
    }
}