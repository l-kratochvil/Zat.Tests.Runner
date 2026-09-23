namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class EnableDebugModePromptScreen(
    TestConfigStore testConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct => ShowPromptAsync(
                new ConfirmationPrompt(Resources.EnableDebugMode_PromptText).ConfigureDefaultOptions(),
                confired =>
                {
                    testConfigStore.IsDebugModeEnabled = confired;
                    return new RenderOutput();
                },
                ct),
        };
}