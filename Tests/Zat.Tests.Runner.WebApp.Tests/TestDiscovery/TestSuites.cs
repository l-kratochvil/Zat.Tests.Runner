namespace Zat.Tests.Runner.WebApp.Tests.TestDiscovery;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// Discovered test suites for tests that need some to select from, but not a particular shape.
/// </summary>
internal static class TestSuites
{
    /// <summary>
    /// Builds a test suite holding one fixture with one test case.
    /// </summary>
    /// <param name="testType">Type of the test case.</param>
    /// <param name="executionPath">Execution path of the test case.</param>
    /// <returns>The test suite.</returns>
    public static TestSuiteEntity WithOneTestCase(TestType testType, string executionPath)
        => new(
            [
                new TestFixtureEntity(
                    [new TestCaseEntity(testType, id: "1", name: "Test", executionPath: executionPath)],
                    testType,
                    name: "Fixture",
                    executionPath: "Suite.Fixture"),
            ],
            testType,
            name: "Suite",
            executionPath: "Suite");
}