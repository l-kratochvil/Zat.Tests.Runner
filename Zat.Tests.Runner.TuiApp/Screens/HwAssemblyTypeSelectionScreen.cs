namespace Zat.Tests.Runner.TuiApp.Screens;

using System;

using Spectre.Console;

using Zat.Tests.Runner.TuiApp.Stores;
using Zat.Z2xxTests.Common;

internal class TestCasesSelectionScreen(
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
            Main = ct =>
            {
                var prompt = new SelectionPrompt<TestedHwAssemblyType>()
                    .Title("# Select testsuites to select testcases from: ")
                    .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .PageSize(10)
                    .AddChoices(Enum.GetValues<TestedHwAssemblyType>());

                return ShowPromptAsync(
                    prompt,
                    selectedHwAssemblyType =>
                    {
                        testConfigStore.TestedHwAssemblyType = selectedHwAssemblyType;
                        return RenderOutput.Default;
                    },
                    ct);
            },
        };
}