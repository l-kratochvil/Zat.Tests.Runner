namespace Zat.Tests.Runner.WebApp.Features.AppLogging;

using Fluxor;
using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

public static class Reducers
{
    private static readonly StateUpdater<AppLoggingState> Updater = new();

    [ReducerMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:Elements should be documented",
        Justification = "Fluxor reducer")]
    public static AppLoggingState OnDataChanged(
        AppLoggingState current, DataChangedAction action)
        => Updater
            .UpdateIfChanged(
                current,
                action.Entries,
                (state, value) => state with { Entries = value })
            .Complete();
}