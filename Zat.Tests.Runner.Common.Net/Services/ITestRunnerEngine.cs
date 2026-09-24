namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common.Model;

public interface ITestRunnerEngine
{
    bool IsRunning { get; }

    Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        Config config);

    void StopTestRun();

    void RegisterTestResultHandler(ITestResultHandler handler);

    public record Config(
        bool IsDebug,
        string? TestedRuntimeVersion,
        HwAssemblyType[]? TestedHwAssemblyTypes,
        IEnumerable<ITestResultHandler>? TestResultHandlers = null);
}