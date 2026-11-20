namespace Zat.Tests.Runner.WebApp.Features.TestExecution.Services;

using DevKit.Core.Interfaces;

using Fluxor;

using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestExecution;

/// <summary>
/// Handles the state changes of the test runner engine and announces them to the store.
/// </summary>
/// <remarks>
/// Scoped to a circuit and started when it opens, so every open browser tab learns of it, whatever it shows.
/// </remarks>
/// <param name="testRunnerEngine">Engine whose test run is followed.</param>
/// <param name="dispatcher">Dispatcher of the circuit's store.</param>
public sealed class TestRunnerEngineStateHandler(
    ITestRunnerEngine testRunnerEngine,
    IDispatcher dispatcher)
    : IInitializable, IDisposable
{
    /// <inheritdoc/>
    public void Initialize()
        => testRunnerEngine.StateChanged += this.OnTestRunStateChanged;

    /// <inheritdoc/>
    public void Dispose()
        => testRunnerEngine.StateChanged -= this.OnTestRunStateChanged;

    private void OnTestRunStateChanged(TestRunState state)
    {
        if (state is TestRunState.Running)
        {
            dispatcher.Dispatch(new TestRunStartedAction());
        }
    }
}
