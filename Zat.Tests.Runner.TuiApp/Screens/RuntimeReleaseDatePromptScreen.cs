namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Stores;

internal class RuntimeReleaseDatePromptScreen(
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
                new TextPrompt<string>(Resources.EnterRuntimeReleaseDate_PromptText).Validate(Validators.IsDate),
                date =>
                {
                    testRunConfigStore.RuntimeReleaseDate = date;
                    return new RenderOutput();
                },
                ct),
        };
}