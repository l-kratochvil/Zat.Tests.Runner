namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Z2xxTests.Common.Model;

public interface ITestRunnerEngine
{
    bool IsRunning { get; }

    Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        Config config);

    void StopTestRun();

    /// <summary>
    /// Configuration of a single test run.
    /// </summary>
    /// <param name="TestResultHandlers">Handlers of this run's results; the engine owns none of its
    /// own, so each application decides who handles them and with what lifetime.</param>
    public record Config(
        bool IsDebug,
        string? TestedRuntimeVersion,
        HwAssemblyType[]? TestedHwAssemblyTypes,
        IEnumerable<ITestResultHandler> TestResultHandlers);
}