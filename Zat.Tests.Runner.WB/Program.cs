using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Moq;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Z2xxTests.Common;

// Experiment: run Zat.Z2xxTests test cases the same way TuiApp does (out-of-process proxy + TestRunnerEngine).
// Usage: Zat.Tests.Runner.WB [<execution-path-prefix>] [--debug]
const string DefaultExecutionPath = "Zat.Z2xxTests.Pertinax6Tests.ProjectManagerTestsuite_BasicFunctions";

var launchDebugger = args.Contains("--debug", StringComparer.OrdinalIgnoreCase);
var executionPath = args.FirstOrDefault(x => !x.StartsWith("--", StringComparison.Ordinal)) ?? DefaultExecutionPath;
var testAssemblyDllPath = Path.Combine(Paths.Directories.TestLibs, "Zat.Z2xxTests.dll");

Console.WriteLine($"Test assembly:  {testAssemblyDllPath}");
Console.WriteLine($"Execution path: {executionPath}");

await using var connector = await NUnitTestRunnerProxyConnector.ConnectAsync(launchDebugger: launchDebugger && !Debugger.IsAttached);

var testSuites = await connector.Proxy.LoadTestAssemblyAsync(testAssemblyDllPath);

TestCaseEntity[] testCases =
[
    .. testSuites
        .SelectMany(x => x.TestFixtures)
        .SelectMany(x => x.TestCases)
        .Where(x => x.ExecutionPath.StartsWith(executionPath, StringComparison.Ordinal)),
];

Console.WriteLine($"Loaded test suites: {testSuites.Length}, selected test cases: {testCases.Length}");
foreach (var testCase in testCases)
{
    Console.WriteLine($"  {testCase.ExecutionPath}");
}

if (testCases.Length == 0)
{
    return 1;
}

var engine = new TestRunnerEngine(
    new TestRunnerBridgeConnector(),
    connector.Proxy,
    new Mock<ILogger<LogSources.TestRun>>().Object);

var results = await engine.RunTestAsync(
    testCases,
    new ITestRunnerEngine.Config(
        IsDebug: false,
        TestedRuntimeVersion: null,
        TestedHwAssemblyTypes: null,
        TestResultHandlers: []));

foreach (var result in results)
{
    Console.WriteLine();
    Console.WriteLine($"=== {result.TestType}: {result.OverallStatus} ===");
    WriteResults(result.TestSuiteResults);
    WriteResults(result.TestFixtureResults);
    WriteResults(result.TestCaseResults);
}

return results.Any(x => x.Failed) ? 2 : 0;

static void WriteResults(IEnumerable<TestEntityResult> results)
{
    foreach (var result in results)
    {
        Console.WriteLine($"[{result.Status}] {result.EntityName}");
        if (result.Detail is { } detail)
        {
            Console.WriteLine(detail.Message);
            Console.WriteLine(detail.StackTrace);
        }
    }
}