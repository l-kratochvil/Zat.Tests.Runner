namespace Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;

/// <summary>
/// Store of application settings for features that only read them.
/// </summary>
/// <remarks>
/// The AppSettings feature owns changing the settings and exposes only this read-only view to other
/// features.
/// </remarks>
public interface IAppSettingsStore : IStore<AppSettingsState>;
