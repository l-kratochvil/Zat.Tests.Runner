namespace Zat.Tests.Runner.WebApp.Features.TestDiscovery;

using Fluxor;

using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

/// <summary>
/// How the test selection answers what has been changed about it.
/// </summary>
// ReSharper disable once UnusedMember.Global
public static class Reducers
{
    private static readonly StateUpdater<TestDiscoveryState> Updater = new();

    [ReducerMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:Elements should be documented",
        Justification = "Fluxor reducer")]
    public static TestDiscoveryState OnSelectionChanged(
        TestDiscoveryState current, SelectionChangedAction action)
        => Updater
            .UpdateIfChanged(
                current,
                action.NewSelectedExecutionPaths,
                (state, value) => state with { SelectedExecutionPaths = value })
            .Complete();
}