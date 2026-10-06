namespace Zat.Tests.Runner.WebApp.Tests.Shared.Stores.TestDiscovery;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

[TestFixture]
public class TestDiscoveryStateExtensionsTests
{
    private const string FirstCasePath = "Suite.Fixture.FirstCase";
    private const string SecondCasePath = "Suite.Fixture.SecondCase";
    private const string FixturePath = "Suite.Fixture";

    [TestCase(new string[0], new string[0])]
    [TestCase(new[] { SecondCasePath }, new[] { SecondCasePath })]
    [TestCase(new[] { SecondCasePath, FirstCasePath }, new[] { FirstCasePath, SecondCasePath })]
    [TestCase(new[] { FirstCasePath, "Suite.Fixture.RemovedCase" }, new[] { FirstCasePath })]
    [TestCase(new[] { FixturePath }, new string[0])]
    public void SelectedTestCases__WhenPathsAreSelected__ThenShouldReturnTestCasesFoundAtThem(
        string[] givenPaths,
        string[] expectedPaths)
    {
        // Given:
        var givenState = new TestDiscoveryState(givenPaths);

        // When:
        var testCases = givenState.SelectedTestCases([CreateTestSuite()]);

        // Then:
        Assert.That(testCases.Select(testCase => testCase.ExecutionPath), Is.EqualTo(expectedPaths));
    }

    private static TestSuiteEntity CreateTestSuite()
        => new(
            [
                new TestFixtureEntity(
                    [
                        CreateTestCase(FirstCasePath),
                        CreateTestCase(SecondCasePath),
                    ],
                    TestType.Application,
                    name: "Fixture",
                    executionPath: FixturePath),
            ],
            TestType.Application,
            name: "Suite",
            executionPath: "Suite");

    private static TestCaseEntity CreateTestCase(string executionPath)
        => new(
            TestType.Application,
            id: executionPath,
            name: executionPath,
            executionPath: executionPath);
}