namespace Zat.Tests.Runner.TuiApp.Screens;

using Spectre.Console;

using System;

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

                testConfigStore
                    .HwAssemblyTypes?
                    .ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedHwAssemblyType =>
                    {
                        testConfigStore.HwAssemblyTypes = [..selectedHwAssemblyType];
                        return RenderOutput.Default;
                    },
                    ct,
                    validator: static selected =>
                        selected.Contains(TestedHwAssemblyType.HW02_BB1M) &&
                        selected.Contains(TestedHwAssemblyType.HW02_BB37M)
                            ? ValidationResult.Error(Resources.SelectingMultipleHw02OptionsNotAllowed)
                            : ValidationResult.Success());
            },
        };
}