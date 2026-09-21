namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class IdeVersionPromptScreen(
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
                new TextPrompt<string>(Resources.EnterIdeIVersion_PromptText),
                version =>
                {
                    testConfigStore.IdeVersion = version;
                    return new RenderOutput();
                },
                ct),
        };
}