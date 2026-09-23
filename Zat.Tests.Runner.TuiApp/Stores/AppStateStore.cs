namespace Zat.Tests.Runner.TuiApp.Stores;

using Zat.Tests.Runner.TuiApp.Model;

internal class AppStateStore : IJsonPersistanceStore<AppState>
{
    private static readonly string JsonPath = Paths.Files.AppState;

    private readonly JsonPersistanceStore<AppState> jsonPersistanceStore = new(CreateDefaultAppState, JsonPath);

    private AppStateStore()
    {
    }

    /// <inheritdoc/>
    public AppState Current
        => this.jsonPersistanceStore.Current;

    /// <summary>
    /// Creates a new instance of the AppStateStore.
    /// </summary>
    /// <returns>A new instance.</returns>
    public static AppStateStore Create()
        => JsonPersistanceStore<AppState>.InitStore(
            JsonPath,
            CreateDefaultAppState,
            model => new AppStateStore().Visit(x => x.Update(model)));

    /// <inheritdoc/>
    public void Update(Func<AppState, AppState> updator)
        => this.jsonPersistanceStore.Update(updator);

    /// <inheritdoc/>
    public void Update(AppState currentModel)
        => this.jsonPersistanceStore.Update(currentModel);

    private static AppState CreateDefaultAppState()
        => new(
            RuntimeVersion: null,
            RuntimeReleaseDate: null,
            IdeVersion: null,
            IdeReleaseDate: null,
            HwAssemblyTypes: null,
            IsTestLinkReportingEnabled: null,
            IsDebugModeEnabled: null);
}