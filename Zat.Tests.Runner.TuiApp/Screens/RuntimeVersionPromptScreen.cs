namespace Zat.Tests.Runner.TuiApp.Screens;

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Zat.Tests.Runner.TuiApp.Stores;

internal partial class RuntimeVersionPromptScreen(
    TestConfigStore testRunConfigStore,
    AppUserSettingsStore appUserSettingsStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var installedRuntimeVersions = GetInstalledRuntimeVersions(appUserSettingsStore);
                if (installedRuntimeVersions.Length == 0)
                {
                    ConsoleUtils.WaitForAnyKeyPress(Resources.NoRuntimesFoundUnderIdeInstallFolder_Message);
                    return Task.FromResult<ShowPromptResult>(CompletedShowPrompt.Default);
                }

                var prompt = new SelectionPrompt<string>()
                    .Title("Select runtime version:")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                    .AddChoices(installedRuntimeVersions);

                return ShowPromptAsync(
                    prompt,
                    version =>
                    {
                        testRunConfigStore.RuntimeVersion = version;
                        return RenderOutput.Default;
                    },
                    ct);
            },
        };

    private static string[] GetInstalledRuntimeVersions(AppUserSettingsStore appUserSettingsStore)
        =>
        [
            ..Directory
                .GetDirectories(appUserSettingsStore.Current.IdeInstallFolderPath)
                .Select(static dir => Path.GetFileName(dir))
                .Where(static dirName => !string.IsNullOrEmpty(dirName) && RuntimeVersion().IsMatch(dirName))
                .OrderBy(static x => int.TryParse(x, out var parsed) ? parsed : char.MaxValue) // full numeric
                .ThenBy(static x => int.TryParse(RuntimeVersion().Match(x).Value, out var parsed) ? parsed : char.MaxValue) // starting with numeric
                .ThenBy(static x => x) // others
                .Reverse()
        ];

    [GeneratedRegex(@"^\d+")]
    private static partial Regex RuntimeVersion();
}