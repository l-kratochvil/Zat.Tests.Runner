namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

using System.Linq;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// Reads the test selection as the test cases it stands for.
/// </summary>
public static class TestDiscoveryStateExtensions
{
    /// <summary>
    /// Finds the test cases of <paramref name="testSuites"/> the selection holds.
    /// </summary>
    /// <remarks>
    /// Paths matching no test case are ignored, so a selection made before the test assemblies
    /// changed stands for as much of itself as still exists.
    /// </remarks>
    /// <param name="state">The test selection.</param>
    /// <param name="testSuites">Test suites the selected test cases are looked up in.</param>
    /// <returns>The selected test cases, in the order of <paramref name="testSuites"/>.</returns>
    public static IReadOnlyList<TestCaseEntity> SelectedTestCases(
        this TestDiscoveryState state,
        IEnumerable<TestSuiteEntity> testSuites)
    {
        var selectedPaths = state.SelectedExecutionPaths.ToHashSet(StringComparer.Ordinal);

        return
        [
            ..testSuites
                .SelectMany(testSuite => testSuite.TestFixtures)
                .SelectMany(testFixture => testFixture.TestCases)
                .Where(testCase => selectedPaths.Contains(testCase.ExecutionPath)),
        ];
    }
}