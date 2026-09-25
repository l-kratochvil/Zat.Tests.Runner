namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class EnableTestLinkReportingPromptScreen(
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
                new ConfirmationPrompt(Resources.EnableTestLinkReporting_PromptText).ConfigureDefaultOptions(),
                confired =>
                {
                    testRunConfigStore.IsTestLinkReportingEnabled = confired;
                    return new RenderOutput();
                },
                ct),
        };
}