namespace Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;

using System.Text.Json.Serialization;

/// <summary>
/// The application settings describing the installation rather than one test run.
/// </summary>
/// <remarks>
/// Shared by every browser because
/// <see cref="Features.AppSettings.Services.AppSettingsStore"/> keeps them in a file on the server.
/// </remarks>
/// <param name="IdeInstallFolderPath">
/// IDE install folder path.
/// </param>
public record AppSettingsState(
    [property: JsonPropertyName("ideInstallFolderPath")]
    string IdeInstallFolderPath);
