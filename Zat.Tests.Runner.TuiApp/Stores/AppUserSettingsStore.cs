namespace Zat.Tests.Runner.TuiApp.Stores;

using Zat.Tests.Runner.TuiApp.Application.Paths;
using Zat.Tests.Runner.TuiApp.Model;

internal class AppUserSettingsStore(string jsonPath) : IJsonPersistanceStore<AppUserSettings>
{
    private const string DefaultIdeInstallationDirPath = @"C:\Program Files (x86)\Pertinax6";

    private readonly JsonPersistanceStore<AppUserSettings> jsonPersistanceStore = new(CreateDefaultAppUserSettings, jsonPath);

    /// <inheritdoc/>
    public AppUserSettings Current
        => this.jsonPersistanceStore.Current;

    public static AppUserSettingsStore Create(IAppPathsProvider paths)
        => JsonPersistanceStore<AppUserSettings>.InitStore(
            paths.Files.UserSettings,
            CreateDefaultAppUserSettings,
            model => new AppUserSettingsStore(paths.Files.UserSettings).Visit(x => x.Update(model)));

    /// <inheritdoc/>
    public void Update(Func<AppUserSettings, AppUserSettings> updator)
        => this.jsonPersistanceStore.Update(updator);

    /// <inheritdoc/>
    public void Update(AppUserSettings currentModel)
        => this.jsonPersistanceStore.Update(currentModel);

    private static AppUserSettings CreateDefaultAppUserSettings()
        => new(DefaultIdeInstallationDirPath);
}