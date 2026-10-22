namespace Zat.Tests.Runner.Common.Model;

/// <summary>
/// The outcome of a test case.
/// </summary>
/// <param name="Id">The identifier of the test case, the same as <see cref="TestCaseEntity.Id"/>.</param>
/// <param name="EntityName">The execution path of the test case.</param>
/// <param name="Status">The status of the test case.</param>
/// <param name="Detail">The detail of the outcome.</param>
/// <param name="TestAttachments">The files the test case attached to its result; empty when omitted.</param>
[Serializable]
public record TestCaseResult(
    string Id,
    string EntityName,
    TestStatus Status,
    Detail? Detail = null,
    TestAttachment[]? TestAttachments = null)
    : TestEntityResult(EntityName, Status, Detail)
{
    /// <summary>
    /// Gets the identifier of the test case, the same as <see cref="TestCaseEntity.Id"/>.
    /// </summary>
    public string Id { get; } = Id;

    /// <summary>
    /// Gets the files the test case attached to its result.
    /// </summary>
    public TestAttachment[] TestAttachments { get; } = TestAttachments ?? [];
}