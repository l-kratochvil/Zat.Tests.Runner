namespace Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// Store of the test tree discovered when the application starts.
/// </summary>
/// <remarks>
/// Shared by every browser because the test assemblies belong to the test machine, not to one
/// tester.
/// </remarks>
public interface INUnitTestRunnerStore
{
    /// <summary>
    /// Gets the discovered test suites. Empty when discovery found none or failed.
    /// </summary>
    IReadOnlyList<TestSuiteEntity> LoadedTestSuites { get; }
}