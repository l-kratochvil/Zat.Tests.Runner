namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// Runs the selected test entities. Only one test run may be in progress on the environment at a time, and its
/// state is shared by everyone who holds the engine.
/// </summary>
public interface ITestRunnerEngine
{
    /// <summary>
    /// Raised whenever <see cref="State"/> changes, possibly on a thread of the caller who caused the change.
    /// </summary>
    event Action<TestRunState>? StateChanged;

    /// <summary>
    /// Gets the current state of the test run.
    /// </summary>
    TestRunState State { get; }

    /// <summary>
    /// Runs <paramref name="testRunEntities"/>.
    /// </summary>
    /// <param name="testRunEntities">The test entities to run.</param>
    /// <param name="config">The configuration of this test run.</param>
    /// <param name="cancellationToken">Stops the test run, the same as <see cref="StopTestRun"/>.</param>
    /// <returns>The test results gathered until the test run ended or was stopped.</returns>
    /// <exception cref="InvalidOperationException">Another test run is in progress.</exception>
    Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        Config config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the test run in progress, whoever started it; does nothing when none is. A test run that doesn't
    /// stop in time is ended forcibly, together with whatever its test cases started.
    /// </summary>
    void StopTestRun();

    /// <summary>
    /// Configuration of a single test run.
    /// </summary>
    /// <param name="TestAssemblyDllPath">The test assembly the test run entities come from.</param>
    /// <param name="TestResultHandlers">Handlers of this run's results; the engine owns none of its
    /// own, so each application decides who handles them and with what lifetime.</param>
    public record Config(
        string TestAssemblyDllPath,
        bool IsDebug,
        string? TestedRuntimeVersion,
        HwAssemblyType[]? TestedHwAssemblyTypes,
        IEnumerable<ITestResultHandler> TestResultHandlers);
}