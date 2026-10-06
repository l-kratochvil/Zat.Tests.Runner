namespace Zat.Tests.Runner.WebApp.Features.TestDiscovery.Services;

using DevKit.Core.Interfaces;

using Fluxor;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

/// <summary>
/// Clears the test selection of a circuit once the test assembly no longer holds the same test entities.
/// </summary>
/// <remarks>
/// Scoped to a circuit and started when it opens, so every open browser tab is cleared, whatever it shows.
/// </remarks>
/// <param name="testTreeStore">Store whose changes are followed.</param>
/// <param name="dispatcher">Dispatcher of the circuit's store.</param>
public sealed class TestSelectionReset(
    ITestTreeStore testTreeStore,
    IDispatcher dispatcher)
    : IInitializable, IDisposable
{
    /// <inheritdoc/>
    public void Initialize()
        => testTreeStore.Changed += this.OnTestTreeChanged;

    /// <inheritdoc/>
    public void Dispose()
        => testTreeStore.Changed -= this.OnTestTreeChanged;

    private void OnTestTreeChanged(TestTreeChange change)
    {
        if (change.ExecutionPathsChanged)
        {
            // The browser remembers the selection the state holds, so it forgets the cleared one too.
            dispatcher.Dispatch(new SelectionChangedAction(
                NewSelectedExecutionPaths: new ValueChange<IReadOnlyList<string>>([])));
        }
    }
}