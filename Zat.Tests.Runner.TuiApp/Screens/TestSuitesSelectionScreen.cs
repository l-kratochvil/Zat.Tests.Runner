namespace Zat.Tests.Runner.TuiApp.Screens;

using System.Linq;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.TuiApp.Extensions;
using Zat.Tests.Runner.TuiApp.Stores;

internal class TestSuitesSelectionScreen(
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

                var prompt = new MultiSelectionPrompt<TestEntity>(TestEntityEqualityComparer)
                    .Title(Resources.SelectTestSuites_PromptText.AsPromptTitle())
                    .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
                    .InstructionsText(SharedTexts.InstructionsHelpText)
                    .PageSize(10)
                    .NotRequired()
                    .UseConverter(x => x.Name);

                foreach (var testsuite in testSuites)
                {
                    prompt.AddChoiceGroup(testsuite, testsuite.TestFixtures);
                }

                testRunConfigStore
                    .SelectedTestEntities
                    .ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedTestSuites =>
                    {
                        testRunConfigStore.SelectedTestEntities = [..selectedTestSuites];
                        return new RenderOutput();
                    },
                    ct);
            },
        };
}