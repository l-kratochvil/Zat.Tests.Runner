namespace Zat.Tests.Runner.Common.Model;

[Serializable]
public class TestEntity(
    TestType testType,
    string name,
    string executionPath)
{
    public TestType TestType { get; } = testType;

    public string Name { get; } = name;

    public string ExecutionPath { get; } = executionPath;

    public static TestEntity Default
        => new(TestType.Unknown, string.Empty, string.Empty);
}