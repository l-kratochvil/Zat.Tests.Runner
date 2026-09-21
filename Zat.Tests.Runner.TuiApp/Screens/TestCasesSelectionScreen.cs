namespace Zat.Tests.Runner.TuiApp.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.TuiApp.Extensions;
using Zat.Tests.Runner.TuiApp.Stores;

internal class TestCasesSelectionScreen(
    TestConfigStore testConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    private static readonly EqualityComparer<TestEntity> TestEntityEqualityComparer = TestEntity.CreateEqualityComparerByName();

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var testSuites = testConfigStore.LoadedTestSuites;
                var prompt = new MultiSelectionPrompt<TestSuiteEntity>(TestEntityEqualityComparer)
                    .Title("# Select testsuites to select testcases from: ")
                    .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .InstructionsText($"[grey]({Resources.PressSpaceToSelectItem})[/]")
                    .PageSize(10)
                    .AddChoices(testSuites)
                    .UseConverter(x => x.Name);

                testConfigStore
                    .SelectedTestEntities
                    .OfType<TestSuiteEntity>()
                    .ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedTestSuites => new RenderOutput(
                        NextScreen: new SelectTestCasesScreen(
                            testSuites: selectedTestSuites,
                            testConfigStore: testConfigStore,
                            homeScreen: this.HomeScreenLazy,
                            exitScreen: this.ExitScreenLazy,
                            settingsScreen: this.SettingsScreenLazy)),
                    ct);
            },
        };

    private class SelectTestCasesScreen(
        IEnumerable<TestSuiteEntity> testSuites,
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
                Main = async ct =>
                {
                    var prompt = new MultiSelectionPrompt<TestEntity>(TestEntityEqualityComparer)
                        .Title("# Select test cases: ")
                        .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                        .InstructionsText($"[grey]({Resources.PressSpaceToSelectItem})[/]")
                        .NotRequired()
                        .PageSize(10)
                        .UseConverter(x => (x as TestCaseEntity)?.Id ?? x.Name);

                    foreach (var testFixture in testSuites.SelectMany(x => x.TestFixtures))
                    {
                        prompt.AddChoiceGroup(testFixture, testFixture.TestCases.OrderBy(x => x.Id));
                    }

                    testConfigStore.SelectedTestEntities.ForEach(entity => prompt.Select(entity));

                    return await ShowPromptAsync(
                        prompt,
                        selectedTestCases =>
                        {
                            testConfigStore.SelectedTestEntities =
                            [
                                ..testConfigStore.SelectedTestEntities
                                    .Where(currentEntity => selectedTestCases.Any(currentEntity.Equals))
                                    .Union(selectedTestCases)
                            ];

                            return RenderOutput.Default;
                        },
                        ct);
                },
            };
    }
}