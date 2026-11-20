namespace Zat.Tests.Runner.Common.Model;

// TODO: Rnm TestSuite
[Serializable]
public class TestFixtureEntity(
    TestCaseEntity[] testCases,
    TestType testType,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath)
{
    public TestCaseEntity[] TestCases { get; } = testCases;
}