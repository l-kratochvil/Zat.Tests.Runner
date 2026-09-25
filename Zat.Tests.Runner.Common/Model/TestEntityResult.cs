namespace Zat.Tests.Runner.Common.Model;

/// <summary>
/// The outcome of a single test entity of any level.
/// A test suite or test fixture has a non-passed <see cref="Status"/> only when the problem originated on it.
/// </summary>
/// <param name="EntityName">The execution path of the test entity.</param>
/// <param name="Status">The status of the test entity.</param>
/// <param name="Detail">The detail of the outcome, <see langword="null"/> when there is nothing to say.</param>
public abstract record TestEntityResult(
    string EntityName,
    TestStatus Status,
    Detail? Detail)
{
    /// <summary>
    /// Gets the execution path of the test entity.
    /// </summary>
    public string EntityName { get; } = EntityName;

    /// <summary>
    /// Gets the status of the test entity.
    /// </summary>
    public TestStatus Status { get; } = Status;

    /// <summary>
    /// Gets the detail of the outcome, <see langword="null"/> when there is nothing to say.
    /// </summary>
    public Detail? Detail { get; } = Detail;
}