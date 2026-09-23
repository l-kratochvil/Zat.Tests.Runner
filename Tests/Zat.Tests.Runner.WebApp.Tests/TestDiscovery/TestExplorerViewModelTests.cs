namespace Zat.Tests.Runner.WebApp.Tests.TestDiscovery;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.WebApp.Features.TestDiscovery.Components;

[TestFixture]
public class TestExplorerViewModelTests
{
    private const string GivenSuitePath = "Suite";
    private const string GivenFirstFixturePath = "Suite.FirstFixture";
    private const string GivenSecondFixturePath = "Suite.SecondFixture";
    private const string GivenFirstCasePath = "Suite.FirstFixture.FirstCase";
    private const string GivenSecondCasePath = "Suite.FirstFixture.SecondCase";
    private const string GivenThirdCasePath = "Suite.SecondFixture.ThirdCase";

    private TestExplorerViewModel unit;

    [SetUp]
    public void SetUp()
    {
        this.unit = new TestExplorerViewModel([CreateTestSuite()]);
    }

    [Test]
    public void Toggle__WhenAFixtureIsSelected__ThenShouldSelectEveryTestCaseUnderIt()
    {
        // Given:
        string[] expectedPaths = [GivenFirstCasePath, GivenSecondCasePath];

        // When:
        this.FindNode(GivenFirstFixturePath).Toggle();

        // Then:
        Assert.That(this.SelectedPaths(), Is.EqualTo(expectedPaths));
    }

    [Test]
    public void Toggle__WhenASelectedFixtureIsToggledAgain__ThenShouldClearEveryTestCaseUnderIt()
    {
        // Given:
        var givenFixture = this.FindNode(GivenFirstFixturePath);
        givenFixture.Toggle();

        // When:
        givenFixture.Toggle();

        // Then:
        Assert.That(this.SelectedPaths(), Is.Empty);
    }

    [Test]
    public void Toggle__WhenAPartlySelectedFixtureIsToggled__ThenShouldSelectTheRestInsteadOfClearingIt()
    {
        // Given:
        // Completing beats undoing: the click after a partial selection is the one that says
        // "and the others too", so it must not throw away what is already selected.
        string[] expectedPaths = [GivenFirstCasePath, GivenSecondCasePath];
        this.FindNode(GivenFirstCasePath).Toggle();

        // When:
        this.FindNode(GivenFirstFixturePath).Toggle();

        // Then:
        Assert.That(this.SelectedPaths(), Is.EqualTo(expectedPaths));
    }

    [TestCaseSource(nameof(CheckStateCases))]
    public TestTreeNodeData.State CheckState__WhenTestCasesAreSelected__ThenShouldFollowThemOnTheFixture(
        string[] givenSelectedPaths)
    {
        // When:
        this.unit.ApplySelection(givenSelectedPaths);

        // Then:
        return this.FindNode(GivenFirstFixturePath).CheckState;
    }

    [Test]
    public void CheckState__WhenTheLastSelectedTestCaseIsCleared__ThenShouldClearTheGroupsAboveIt()
    {
        // Given:
        var givenTestCase = this.FindNode(GivenFirstCasePath);
        givenTestCase.Toggle();

        // When:
        givenTestCase.Toggle();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.FindNode(GivenFirstFixturePath).CheckState, Is.EqualTo(TestTreeNodeData.State.Unchecked));
            Assert.That(this.FindNode(GivenSuitePath).CheckState, Is.EqualTo(TestTreeNodeData.State.Unchecked));
        }
    }

    [Test]
    public void CheckState__WhenOnlyOneFixtureIsFullySelected__ThenShouldLeaveTheSuitePartlySelected()
    {
        // When:
        this.FindNode(GivenFirstFixturePath).Toggle();

        // Then:
        Assert.That(this.FindNode(GivenSuitePath).CheckState, Is.EqualTo(TestTreeNodeData.State.Mixed));
    }

    [Test]
    public void SelectedTestCases__WhenTheWholeSuiteIsSelected__ThenShouldHoldTestCasesOnly()
    {
        // Given:
        // Groups are never part of the selection: a suite path would run everything under it, which
        // is not what the user ticked.
        string[] expectedPaths = [GivenFirstCasePath, GivenSecondCasePath, GivenThirdCasePath];

        // When:
        this.FindNode(GivenSuitePath).Toggle();

        // Then:
        Assert.That(this.SelectedPaths(), Is.EqualTo(expectedPaths));
    }

    [Test]
    public void ApplySelection__WhenAPathMatchesNoTestCase__ThenShouldIgnoreIt()
    {
        // Given:
        // A selection remembered before the test assemblies changed restores as much of itself as
        // still exists, instead of claiming to run a test that is gone.
        string[] givenPaths = [GivenFirstCasePath, "Suite.FirstFixture.RemovedCase"];
        string[] expectedPaths = [GivenFirstCasePath];

        // When:
        this.unit.ApplySelection(givenPaths);

        // Then:
        Assert.That(this.SelectedPaths(), Is.EqualTo(expectedPaths));
    }

    [Test]
    public void ApplySelection__WhenATestCaseIsSelected__ThenShouldOpenTheGroupsHidingIt()
    {
        // Given:
        // A restored selection nobody can see is indistinguishable from none.
        this.FindNode(GivenFirstFixturePath).IsExpanded = false;

        // When:
        this.unit.ApplySelection([GivenFirstCasePath]);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.FindNode(GivenFirstFixturePath).IsExpanded, Is.True);
            Assert.That(this.FindNode(GivenSecondFixturePath).IsExpanded, Is.False);
        }
    }

    private static IEnumerable<TestCaseData> CheckStateCases()
    {
        const string Prefix = nameof(TestTreeNodeData.CheckState);

        yield return new TestCaseData(new object[] { Array.Empty<string>() })
            .SetName(Prefix + "__WhenNoTestCaseIsSelected__ThenShouldBe_Unchecked")
            .Returns(TestTreeNodeData.State.Unchecked);

        yield return new TestCaseData(new object[] { new[] { GivenFirstCasePath } })
            .SetName(Prefix + "__WhenSomeTestCasesAreSelected__ThenShouldBe_Mixed")
            .Returns(TestTreeNodeData.State.Mixed);

        yield return new TestCaseData(
                new object[] { new[] { GivenFirstCasePath, GivenSecondCasePath } })
            .SetName(Prefix + "__WhenEveryTestCaseIsSelected__ThenShouldBe_Checked")
            .Returns(TestTreeNodeData.State.Checked);
    }

    private static TestSuiteEntity CreateTestSuite()
        => new(
            [
                new TestFixtureEntity(
                    [
                        CreateTestCase(name: "First case", executionPath: GivenFirstCasePath),
                        CreateTestCase(name: "Second case", executionPath: GivenSecondCasePath),
                    ],
                    TestType.Application,
                    name: "First fixture",
                    executionPath: GivenFirstFixturePath),
                new TestFixtureEntity(
                    [CreateTestCase(name: "Third case", executionPath: GivenThirdCasePath)],
                    TestType.Application,
                    name: "Second fixture",
                    executionPath: GivenSecondFixturePath),
            ],
            TestType.Application,
            name: "Suite",
            executionPath: GivenSuitePath);

    private static TestCaseEntity CreateTestCase(string name, string executionPath)
        => new(
            TestType.Application,
            id: executionPath,
            name: name,
            executionPath: executionPath);

    private TestTreeNodeData FindNode(string executionPath)
        => this.unit.Roots
            .SelectMany(root => root.SelfAndDescendants())
            .Single(node => node.ExecutionPath == executionPath);

    private IEnumerable<string> SelectedPaths()
        => this.unit.SelectedTestCases.Select(testCase => testCase.ExecutionPath);
}