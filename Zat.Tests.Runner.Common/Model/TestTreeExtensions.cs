namespace Zat.Tests.Runner.Common.Model;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Reads a test tree as a whole.
/// </summary>
public static class TestTreeExtensions
{
    /// <summary>
    /// Lists every test entity of <paramref name="testSuites"/>, whatever its level.
    /// </summary>
    /// <param name="testSuites">The test tree.</param>
    /// <returns>The test suites, test fixtures and test cases of <paramref name="testSuites"/>.</returns>
    public static IEnumerable<TestEntity> AllTestEntities(this IEnumerable<TestSuiteEntity> testSuites)
        => testSuites.SelectMany(testSuite => testSuite.TestFixtures
            .SelectMany(testFixture => testFixture.TestCases)
            .Concat<TestEntity>(testSuite.TestFixtures)
            .Append(testSuite));
}