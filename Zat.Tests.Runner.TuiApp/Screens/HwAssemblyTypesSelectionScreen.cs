namespace Zat.Tests.Runner.TuiApp.Screens;

using System;

using Spectre.Console;

using Zat.Tests.Runner.TuiApp.Stores;
using Zat.Z2xxTests.Common;

internal class HwAssemblyTypesSelectionScreen(
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
                var prompt = new MultiSelectionPrompt<TestedHwAssemblyType>()
                    .Title(Resources.HwAssemblyTypes_ChoiceText.AsPromptTitle())
                    .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
                    .PageSize(10)
                    .InstructionsText(SharedTexts.InstructionsHelpText)
                    .AddChoices(Enum.GetValues<TestedHwAssemblyType>());

                return ShowPromptAsync(
                    prompt,
                    selectedHwAssemblyType =>
                    {
                        testConfigStore.HwAssemblyTypes = [..selectedHwAssemblyType];
                        return RenderOutput.Default;
                    },
                    ct);
            },
        };
}