namespace Zat.Tests.Runner.TuiApp.Screens;

internal class ExitScreen(
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    // TODO:
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct => ShowPromptAsync(
                new ConfirmationPrompt($"{Resources.ExitApp}?").ConfigureDefaultOptions(),
                confirmed => confirmed ? new RenderOutput(Exit: true) : new RenderOutput(),
                ct),
        };
}