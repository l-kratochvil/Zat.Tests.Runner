namespace Zat.Tests.Runner.Common.Model;

// TODO: Rnm TestCase
public class TestCaseEntity(
    TestType testType,
    string id,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath)
{
    /// <summary>
    /// Gets the unique identifier of the test case.
    /// </summary>
    public string Id { get; } = id;
}