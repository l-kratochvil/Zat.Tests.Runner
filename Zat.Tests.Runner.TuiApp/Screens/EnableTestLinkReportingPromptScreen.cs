namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class EnableTestLinkReportingPromptScreen(
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
                new ConfirmationPrompt(Resources.EnableTestLinkReporting_PromptText).ConfigureDefaultOptions(),
                send =>
                {
                    testConfigStore.IsTestLinkReportingEnabled = send;
                    return new RenderOutput();
                },
                ct),
        };
}