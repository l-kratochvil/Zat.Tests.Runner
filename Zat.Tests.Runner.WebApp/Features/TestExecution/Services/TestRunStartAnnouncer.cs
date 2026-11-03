namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Services;

using DevKit.Core.Interfaces;

using Fluxor;

using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestResultReporting;

/// <summary>
/// Clears the result panel of a circuit when a new test run starts.
/// </summary>
public sealed class TestResultReset(
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