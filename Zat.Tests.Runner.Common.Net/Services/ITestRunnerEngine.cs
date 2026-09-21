namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common.Model;

public interface ITestRunnerEngine
{
    bool IsRunning { get; }

    Task<TestRunResult> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities, TestConfig testConfig);

    void StopTestRun();
}