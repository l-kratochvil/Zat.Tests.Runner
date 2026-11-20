namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class BetaVersionPromptScreen(
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
                new TextPrompt<string>(Resources.EnterBetaVersion_PromptText).Validate(Validators.IsInt),
                date =>
                {
                    testRunConfigStore.BetaVersion = date;
                    return new RenderOutput();
                },
                ct),
        };
}