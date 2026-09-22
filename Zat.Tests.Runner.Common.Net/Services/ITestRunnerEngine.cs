namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common;

public interface ITestRunnerEngine
{
    bool IsRunning { get; }

    Task<TestResult[]> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities,
        string? testedRuntimeVersion,
        TestedHwAssemblyType[]? testedHwAssemblyTypes,
        bool isDebug,
        IEnumerable<ITestResultHandler>? testResultHandlers = null);

    void StopTestRun();

    void RegisterTestResultHandler(ITestResultHandler handler);
}