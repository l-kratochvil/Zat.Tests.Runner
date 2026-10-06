namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// A replacement of the test tree.
/// </summary>
/// <param name="TestSuites">The test suites that are there now.</param>
/// <param name="ExecutionPathsChanged">Whether the replaced test tree held other test entities than this one, so a
/// test selection made in it may no longer be what it was.</param>
public sealed record TestTreeChange(IReadOnlyList<TestSuiteEntity> TestSuites, bool ExecutionPathsChanged);