namespace Zat.Tests.Runner.TuiApp.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Utils;
using Zat.Tests.Runner.TuiApp.Extensions;
using Zat.Tests.Runner.TuiApp.Stores;

internal class TestCasesSelectionScreen(
    TestRunConfigStore testRunConfigStore,
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
                var testSuites = testRunConfigStore.LoadedTestSuites.ToArray();
                var selectedTestEntities = testRunConfigStore.SelectedTestEntities.ToArray();
                var prompt = new MultiSelectionPrompt<TestEntity>(TestEntityEqualityComparer)
                    .Title(Resources.SelectTestSuitesToSelectTestCases_PromptText.AsPromptTitle())
                    .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
                    .InstructionsText(SharedTexts.InstructionsHelpText)
                    .PageSize(10)
                    .UseConverter(x => LocalizationUtils.MapTextToLocalized(x.Name));

                foreach (var testSuite in testSuites)
                {
                    prompt.AddChoiceGroup(testSuite, testSuite.TestFixtures);
                }

                testSuites
                    .SelectMany(testSuite => testSuite.TestFixtures)
                    .Where(testFixture => ContainsAny(
                        entities: selectedTestEntities,
                        searchedEntities: [testFixture, .. testFixture.TestCases]))
                    .ForEach(testFixture => prompt.Select(testFixture));

                return ShowPromptAsync(
                    prompt,
                    selectedEntities =>
                    {
                        // Empty test suites come back as leaves; neither they nor empty test fixtures have test cases to offer.
                        var testFixtures = selectedEntities
                            .OfType<TestFixtureEntity>()
                            .Where(testFixture => testFixture.TestCases.Length > 0)
                            .ToArray();

                        return testFixtures.Length == 0
                            ? RenderOutput.Default
                            : new RenderOutput(
                                NextScreen: new SelectTestCasesScreen(
                                    testFixtures: testFixtures,
                                    testRunConfigStore: testRunConfigStore,
                                    homeScreen: this.HomeScreenLazy,
                                    exitScreen: this.ExitScreenLazy,
                                    settingsScreen: this.SettingsScreenLazy));
                    },
                    ct);
            },
        };

    private static bool ContainsAny(
        IEnumerable<TestEntity> entities,
        IEnumerable<TestEntity> searchedEntities)
        => entities.Any(entity => searchedEntities.Contains(entity, TestEntityEqualityComparer));

    private class SelectTestCasesScreen(
        TestFixtureEntity[] testFixtures,
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
                Main = async ct =>
                {
                    var prompt = new MultiSelectionPrompt<TestEntity>(TestEntityEqualityComparer)
                        .Title(Resources.SelectTestCases_PromptText.AsPromptTitle())
                        .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
                        .InstructionsText(SharedTexts.InstructionsHelpText)
                        .NotRequired()
                        .PageSize(10)
                        .UseConverter(x => (x as TestCaseEntity)?.Id ?? x.Name);

                    var selectedTestEntities = testRunConfigStore.SelectedTestEntities.ToArray();

                    foreach (var testFixture in testFixtures)
                    {
                        prompt.AddChoiceGroup(testFixture, testFixture.TestCases.OrderBy(x => x.Id));

                        // A whole selected test fixture means all of its test cases are selected.
                        var isTestFixtureSelected = ContainsAny(
                            entities: selectedTestEntities,
                            searchedEntities: [testFixture]);

                        testFixture.TestCases
                            .Where(testCase => isTestFixtureSelected || ContainsAny(
                                entities: selectedTestEntities,
                                searchedEntities: [testCase]))
                            .ForEach(testCase => prompt.Select(testCase));
                    }

                    TestEntity[] shownEntities =
                    [
                        .. testFixtures,
                        .. testFixtures.SelectMany(testFixture => testFixture.TestCases),
                    ];

                    return await ShowPromptAsync(
                        prompt,
                        selectedTestCases =>
                        {
                            testRunConfigStore.SelectedTestEntities =
                            [
                                .. selectedTestEntities
                                    .Where(entity => !shownEntities.Contains(entity, TestEntityEqualityComparer))
                                    .Union(selectedTestCases, TestEntityEqualityComparer)
                            ];

                            return RenderOutput.Default;
                        },
                        ct);
                },
            };
    }
}