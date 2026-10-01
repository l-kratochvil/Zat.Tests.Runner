namespace Zat.Tests.Runner.TuiApp.Stores;

using Zat.Tests.Runner.TuiApp.Application.Paths;
using Zat.Tests.Runner.TuiApp.Model;

internal class AppStateStore(string jsonPath) : IJsonPersistanceStore<AppState>
{
    private readonly JsonPersistanceStore<AppState> jsonPersistanceStore = new(CreateDefaultAppState, jsonPath);

    /// <inheritdoc/>
    public AppState Current
        => this.jsonPersistanceStore.Current;

    /// <summary>
    /// Creates a new instance of the AppStateStore.
    /// </summary>
    /// <param name="paths">Provider of the application paths, naming the application state file.</param>
    /// <returns>A new instance.</returns>
    public static AppStateStore Create(IAppPathsProvider paths)
        => JsonPersistanceStore<AppState>.InitStore(
            paths.Files.AppState,
            CreateDefaultAppState,
            model => new AppStateStore(paths.Files.AppState).Visit(x => x.Update(model)));

    /// <inheritdoc/>
    public void Update(Func<AppState, AppState> updator)
        => this.jsonPersistanceStore.Update(updator);

    /// <inheritdoc/>
    public void Update(AppState currentModel)
        => this.jsonPersistanceStore.Update(currentModel);

    private static AppState CreateDefaultAppState()
        => new();
}