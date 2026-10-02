namespace Zat.Tests.Runner.Common.Net.Tests;

using System.Diagnostics;
using System.Reflection;

using DevKit.Core.Extensions.Types;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Net.Tests.Application.Paths;
using Zat.Tests.Runner.Common.Services;

[TestFixture]
public class NUnitTestRunnerRpcTests
{
    private const bool LaunchDebugger = false; // Enable debugger launch only if necessary to not spawn "attach to process" prompts
    private const string TestAssemblyNet461Name = "NUnitTestAssembly.Net461";
    private const string TestAssemblyNet481Name = "NUnitTestAssembly.Net481";
    private const string Net481FailingTestCasePath = TestAssemblyNet481Name + ".SampleTestSuite.Fail";

    private static readonly string NUnitTestAssembliesDirPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetAssemblyDirectoryPath(),
        "NUnitTestAssemblies");

    private static readonly string TestAssemblyNet461DllPath = Path.Combine(
        NUnitTestAssembliesDirPath,
        TestAssemblyNet461Name,
        $"{TestAssemblyNet461Name}.dll");

    private static readonly string TestAssemblyNet481DllPath = Path.Combine(
        NUnitTestAssembliesDirPath,
        TestAssemblyNet481Name,
        $"{TestAssemblyNet481Name}.dll");

    private readonly TestAppPathsProvider appPathsProvider = new(TestContext.CurrentContext.TestDirectory);

    private NUnitTestRunnerProxyConnector connector = null!;

    [SetUp]
    public async Task SetUp()
    {
        this.connector = await NUnitTestRunnerProxyConnector.ConnectAsync(launchDebugger: LaunchDebuggerIfRequired());
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.connector.DisposeAsync();
    }

    [TestCaseSource(nameof(TestAssemblyCases))]
    public async Task LoadTestAssemblyAsync__WhenLoadingTestAssembly__ThenShouldReturnTestTree(string testAssemblyDllPath)
    {
        // Given
        var unit = this.connector.Proxy;

        // When & Then
        await AssertTestAssemblyLoadedAsync(unit, testAssemblyDllPath);
    }

    [TestCaseSource(nameof(TestAssemblyCases))]
    public async Task RunTestAsync__WhenRunWithAllTestCases__ThenShouldReturnResultOfEveryTestCase(string testAssemblyDllPath)
    {
        // Given
        var unit = this.connector.Proxy;
        var testCases = GetTestCases(await LoadTestAssemblyAsync(unit, testAssemblyDllPath));

        // When
        var result = await unit.RunTestAsync(testCases);

        // Then
        var testCaseResults = GetTestCaseResults(result);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                testCaseResults.Select(x => (x.EntityName, x.Id)),
                Is.EquivalentTo(testCases.Select(x => (x.ExecutionPath, x.Id))));
            Assert.That(testCaseResults, Has.None.Matches<TestCaseResult>(x => x.Status == TestStatus.Unknown));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenRunWithFailingTestCase__ThenShouldReturnFailureDetail()
    {
        // Given
        var unit = this.connector.Proxy;
        var testCase = GetTestCases(await LoadTestAssemblyAsync(unit, TestAssemblyNet481DllPath))
            .Single(x => x.ExecutionPath == Net481FailingTestCasePath);

        // When
        var result = await unit.RunTestAsync([testCase]);

        // Then
        var testCaseResult = GetTestCaseResults(result).Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testCaseResult.Status, Is.EqualTo(TestStatus.Failure));
            Assert.That(testCaseResult.Detail?.Message, Does.Contain("FAILURE REASON"));
        }
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithZatTestsAssembly()
    {
        // TODO: "Test libs" folder content should be copied to the output folder of this test project, so that the test can run on any machine without manual setup.
        var zatTestsAssemblyPath = Path.Combine(this.appPathsProvider.Files.MainAssemblyDll);

        if (!File.Exists(zatTestsAssemblyPath))
        {
            throw new FileNotFoundException(zatTestsAssemblyPath);
        }

        // Given
        var unit = this.connector.Proxy;

        // When & Then
        await AssertTestAssemblyLoadedAsync(unit, zatTestsAssemblyPath);
    }

    private static IEnumerable<TestCaseData> TestAssemblyCases()
    {
        yield return new TestCaseData(TestAssemblyNet461DllPath).SetArgDisplayNames(TestAssemblyNet461Name);
        yield return new TestCaseData(TestAssemblyNet481DllPath).SetArgDisplayNames(TestAssemblyNet481Name);
    }

    private static async Task AssertTestAssemblyLoadedAsync(
        INUnitTestRunnerProxy proxy,
        string testAssemblyDllPath)
    {
        var result = await LoadTestAssemblyAsync(proxy, testAssemblyDllPath);

        // Then
        AssertTestTreeLoaded(result);
    }

    private static Task<TestSuiteEntity[]> LoadTestAssemblyAsync(INUnitTestRunnerProxy proxy, string testAssemblyDllPath)
        => File.Exists(testAssemblyDllPath)
            ? proxy.LoadTestAssemblyAsync(testAssemblyDllPath)
            : throw new FileNotFoundException(testAssemblyDllPath);

    private static TestCaseEntity[] GetTestCases(TestSuiteEntity[] testSuites)
        => [.. testSuites.SelectMany(x => x.TestFixtures).SelectMany(x => x.TestCases)];

    private static TestCaseResult[] GetTestCaseResults(ProxyTestResult result)
        =>
        [
            .. result.TestSuiteResults
                .SelectMany(x => x.TestFixtureResults)
                .SelectMany(x => x.TestCaseResults)
        ];

    private static void AssertTestTreeLoaded(TestSuiteEntity[] testSuites)
    {
        var testFixtures = testSuites.SelectMany(x => x.TestFixtures).ToArray();
        var testCases = testFixtures.SelectMany(x => x.TestCases).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(testSuites, Is.Not.Empty);
            Assert.That(testFixtures, Is.Not.Empty);
            Assert.That(testCases, Is.Not.Empty);
        }
    }

    private static bool LaunchDebuggerIfRequired()
        => LaunchDebugger && !Debugger.IsAttached;
}