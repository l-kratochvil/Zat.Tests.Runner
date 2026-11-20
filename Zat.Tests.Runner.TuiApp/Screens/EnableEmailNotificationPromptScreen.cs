namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class EnableEmailNotificationPromptScreen(
    TestRunConfigStore testRunConfigStore,
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
                new ConfirmationPrompt(Resources.EnableEmailNotification_PromptText).ConfigureDefaultOptions(),
                confirmed =>
                {
                    testRunConfigStore.IsEmailNotificationEnabled = confirmed;
                    return new RenderOutput();
                },
                ct),
        };
}