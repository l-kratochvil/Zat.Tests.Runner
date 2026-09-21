namespace Zat.Tests.Runner.TuiApp.Screens;

using System;
using System.Collections.Generic;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.TuiApp.Common;
using Zat.Tests.Runner.TuiApp.Stores;
using Zat.Z2xxTests.Common;

internal sealed class HomeScreen(
    TestConfigStore testConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen,
    RuntimeVersionPromptScreen runtimeVersionPromptScreen,
    IdeVersionPromptScreen ideVersionPromptScreen,
    TestSuitesSelectionScreen testEntitiesFromTestsuitesPromptScreen,
    TestCasesSelectionScreen testEntitiesFromTestCasesPromptScreen,
    HwAssemblyTypeSelectionScreen hwAssemblyTypeSelectionScreen,
    RunTestScreen runTestScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override Configuration Config { get; init; } = new()
    {
        IsHomeCommandEnabled = false,
        IsBackCommandEnabled = false,
    };

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var choices = this.GetChoices();
                var prompt = new SelectionPrompt<Choice<IScreen>>()
                    .Title(string.Empty) // The console is buggy if no title is set
                    .PageSize(10)
                    .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .AddChoices(choices)
                    .UseConverter(choice => choice.Text)
                    .HighlightStyle(new Style(foreground: Color.Aqua, decoration: Spectre.Console.Decoration.Bold));

                return ShowPromptAsync(
                    prompt,
                    choice => new RenderOutput(NextScreen: choice.Value),
                    ct);
            },
            Info = () =>
            {
                var testFixtures = testConfigStore
                    .SelectedTestEntities.OfType<TestFixtureEntity>().ToArray();
                var testCases = testConfigStore
                    .SelectedTestEntities.OfType<TestCaseEntity>().ToArray();

                TestEntity[] testEntities = [];

                if (testFixtures.Length != 0)
                {
                    testEntities = testFixtures;
                }
                else if (testCases is { Length: > 0 and < 10 })
                {
                    testEntities = testCases;
                }

                var anyTestEntities = testEntities.Length != 0;

                Write(new Rule("INFO").LeftJustified());
                WriteLine();
                MarkupLine(anyTestEntities
                    ? $"[bold]# {Resources.TestEntities}:[/] [yellow]{Resources.Selected}[/]"
                    : $"[bold]# {Resources.TestEntities}:[/] [gray]{Resources.Unselected}[/]");

                if (anyTestEntities)
                {
                    MarkupLine($"[bold]# {Resources.SelectedEntitiesCount}: [yellow]{testEntities.Length}[/][/]");
                    MarkupLine($"[bold]# {Resources.SelectedEntities}:[/]");
                }

                foreach (var testEntity in testEntities)
                {
                    MarkupLine($"  [yellow]{(testEntity as TestCaseEntity)?.Id ?? testEntity.Name}[/]");
                }

                WriteLine();
                Write(new Rule());
            },
        };

    private IEnumerable<Choice<IScreen>> GetChoices()
    {
        yield return new Choice<IScreen>(
            value: runtimeVersionPromptScreen,
            displayText: Resources.RuntimeVersion_ChoiceText,
            displayValue: testConfigStore.RuntimeVersion);

        if (testConfigStore.RuntimeVersion is null)
        {
            yield break;
        }

        if (Choice.InitChoice<IScreen>(
                ideVersionPromptScreen,
                Resources.IdeVersion_ChoiceText,
                testConfigStore.IdeVersion)
            .TryGetValue(out var ideVersionChoice))
        {
            yield return ideVersionChoice;
        }

        if (testConfigStore.IdeVersion is null)
        {
            yield break;
        }

        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestsuitesPromptScreen,
                Resources.SelectTestSuites_ChoiceText,
                null)
            .TryGetValue(out var selectTestSuiteChoice))
        {
            yield return selectTestSuiteChoice;
        }

        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestCasesPromptScreen,
                Resources.SelectTestCases_ChoiceText,
                null)
            .TryGetValue(out var selectTestCasesChoice))
        {
            yield return selectTestCasesChoice;
        }

        if (!testConfigStore.SelectedTestEntities.Any())
        {
            yield break;
        }

        if (Choice.InitChoice<IScreen>(
                hwAssemblyTypeSelectionScreen,
                Resources.SelectHwAssemblyType_ChoiceText,
                testConfigStore.TestedHwAssemblyType.ToString(),
                () => testConfigStore.IsRuntimeTest)
            .TryGetValue(out var selectHwAssemblyTypeChoice))
        {
            yield return selectHwAssemblyTypeChoice;
        }

        if (testConfigStore is
            {
                IsRuntimeTest: true,
                TestedHwAssemblyType: null or TestedHwAssemblyType.Unknown
            })
        {
            yield break;
        }

        if (string.IsNullOrEmpty(testConfigStore.IdeVersion) ||
            string.IsNullOrEmpty(testConfigStore.RuntimeVersion) ||
            !testConfigStore.SelectedTestEntities.Any())
        {
            throw new InvalidOperationException("Invalid config (some required values are missing)");
        }

        yield return new Choice<IScreen>(
            runTestScreen,
            displayText: Resources.RunTest_ChoiceText);
    }
}