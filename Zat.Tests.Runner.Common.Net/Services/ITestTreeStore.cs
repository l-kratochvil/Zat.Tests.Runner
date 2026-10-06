namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// Store of the test tree discovered from the test assembly, kept current as the test assembly changes on disk.
/// </summary>
/// <remarks>
/// Shared by everyone who chooses tests, because the test assembly belongs to the test machine.
/// </remarks>
public interface ITestTreeStore
{
    /// <summary>
    /// Raised whenever <see cref="TestSuites"/> is replaced, on whatever thread replaced it.
    /// </summary>
    event Action<TestTreeChange>? Changed;

    /// <summary>
    /// Gets the discovered test suites. Empty before the first discovery and when the last one failed.
    /// </summary>
    IReadOnlyList<TestSuiteEntity> TestSuites { get; }

    /// <summary>
    /// Discovers the test tree and from then on discovers it again whenever the test assembly changes.
    /// </summary>
    /// <remarks>
    /// Only this discovery reports its failure to the caller. A later one that fails leaves the store empty and
    /// says why in the log.
    /// </remarks>
    /// <param name="cancellationToken">Abandons the discovery.</param>
    /// <returns>A task that completes once the test tree is discovered.</returns>
    Task DiscoverAsync(CancellationToken cancellationToken = default);
}
