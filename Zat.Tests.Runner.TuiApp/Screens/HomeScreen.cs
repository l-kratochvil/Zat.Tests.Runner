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
    HwAssemblyTypesSelectionScreen hwAssemblyTypesSelectionScreen,
    EnableTestLinkReportingPromptScreen enableTestLinkReportingPromptScreen,
    EnableDebugModePromptScreen enableDebugModePromptScreen,
    RuntimeReleaseDatePromptScreen runtimeReleaseDatePromptScreen,
    IdeReleaseDatePromptScreen ideReleaseDatePromptScreen,
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
                    .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
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
        // Runtime version choice
        yield return new Choice<IScreen>(
            value: runtimeVersionPromptScreen,
            displayText: Resources.RuntimeVersion_ChoiceText,
            displayValue: testConfigStore.RuntimeVersion);

        // TODO: Runtime version should be optional (PDP tests can be run without runtime version)
        // REQUIRED: Runtime version
        if (testConfigStore.RuntimeVersion is null)
        {
            yield break;
        }

        // Test suites selection choice
        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestsuitesPromptScreen,
                Resources.SelectTestSuites_ChoiceText)
            .TryGetValue(out var selectTestSuiteChoice))
        {
            yield return selectTestSuiteChoice;
        }

        // Test cases selection choice
        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestCasesPromptScreen,
                Resources.SelectTestCases_ChoiceText)
            .TryGetValue(out var selectTestCasesChoice))
        {
            yield return selectTestCasesChoice;
        }

        // REQUIRED: Test entities selection
        if (!testConfigStore.SelectedTestEntities.Any())
        {
            yield break;
        }

        // HW assembly type selection choice
        if (Choice.InitChoice<IScreen>(
                hwAssemblyTypesSelectionScreen,
                Resources.HwAssemblyTypes_ChoiceText,
                testConfigStore.HwAssemblyTypes is null ? null : string.Join(", ", testConfigStore.HwAssemblyTypes),
                () => testConfigStore.RuntimeTestEntitiesSelected)
            .TryGetValue(out var selectHwAssemblyTypesChoice))
        {
            yield return selectHwAssemblyTypesChoice;
        }

        // REQUIRED: HW assembly type selection when runtime test entities are selected
        if (testConfigStore is
            {
                RuntimeTestEntitiesSelected: true,
                HwAssemblyTypes: null
            })
        {
            yield break;
        }

        // Enable Test Link reporting choice
        if (Choice.InitChoice<IScreen>(
                enableTestLinkReportingPromptScreen,
                Resources.EnableTestLinkReporting_PromptText,
                testConfigStore.IsTestLinkReportingEnabled)
            .TryGetValue(out var enableTestLinkReportingPromptScreenChoice))
        {
            yield return enableTestLinkReportingPromptScreenChoice;
        }

        // REQUIRED: Enable TestLink reporting
        if (testConfigStore.IsTestLinkReportingEnabled is null)
        {
            yield break;
        }

        if (testConfigStore.IsTestLinkReportingEnabled.HasValue &&
            testConfigStore.IsTestLinkReportingEnabled.Value)
        {
            // IDE version choice
            if (Choice.InitChoice<IScreen>(
                    ideVersionPromptScreen,
                    Resources.IdeVersion_ChoiceText,
                    testConfigStore.IdeVersion)
                .TryGetValue(out var ideVersionChoice))
            {
                yield return ideVersionChoice;
            }

            // IDE release date choice
            if (Choice.InitChoice<IScreen>(
                    ideReleaseDatePromptScreen,
                    Resources.IdeReleaseDate_ChoiceText,
                    testConfigStore.IdeReleaseDate)
                .TryGetValue(out var ideReleaseDateChoice))
            {
                yield return ideReleaseDateChoice;
            }

            // Runtime release date choice
            if (Choice.InitChoice<IScreen>(
                    runtimeReleaseDatePromptScreen,
                    Resources.RuntimeReleaseDate_ChoiceText,
                    testConfigStore.RuntimeReleaseDate)
                .TryGetValue(out var runtimeReleaseDateChoice))
            {
                yield return runtimeReleaseDateChoice;
            }

            // REQUIRED: IDE version
            if (testConfigStore.IdeVersion is null)
            {
                yield break;
            }
        }

        // Enable debug mode choice
        if (Choice.InitChoice<IScreen>(
                enableDebugModePromptScreen,
                Resources.EnableDebugMode_PromptText,
                testConfigStore.IsDebugModeEnabled)
            .TryGetValue(out var enableDebugModePromptScreenChoice))
        {
            yield return enableDebugModePromptScreenChoice;
        }

        // Validation of required configuration values
        if (string.IsNullOrEmpty(testConfigStore.RuntimeVersion) ||
            !testConfigStore.SelectedTestEntities.Any())
        {
            throw new InvalidOperationException("Invalid configuration (some required values are missing)");
        }

        yield return new Choice<IScreen>(
            runTestScreen,
            displayText: Resources.RunTest_ChoiceText);
    }
}