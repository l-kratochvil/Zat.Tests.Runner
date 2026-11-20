namespace Zat.Tests.Runner.TuiApp.Model;

using System.Text.Json.Serialization;

internal record AppUserSettings(
    [property: JsonPropertyName("ideInstallFolderPath")]
    string IdeInstallFolderPath);