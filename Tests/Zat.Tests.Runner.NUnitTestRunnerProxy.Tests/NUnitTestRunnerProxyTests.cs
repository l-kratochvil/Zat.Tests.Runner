namespace Zat.Tests.Runner.NUnitTestRunnerProxy.Tests;

using System.Reflection;

using DevKit.Core.Extensions.Types;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;

// TODO: Test GetIsAssemblyLoadedAsync, GetIsTestRunningAsync, etc.
// TODO: Change to Net10 and communicate with proxy via StreamJsonRpc (as the production proxy client does) instead of directly calling the proxy implementation.
public class NUnitTestRunnerProxyTests
{
    private const string TestAssemblyNet461Name = "NUnitTestAssembly.Net461";
    private const string TestAssemblyNet481Name = "NUnitTestAssembly.Net481";

    private const string Net481TestSuitePath = TestAssemblyNet481Name;
    private const string SampleTestFixturePath = Net481TestSuitePath + ".SampleTestSuite";
    private const string OneTimeSetUpFailingTestFixturePath = Net481TestSuitePath + ".OneTimeSetUpFailingFixture";
    private const string OneTimeTearDownFailingTestFixturePath = Net481TestSuitePath + ".OneTimeTearDownFailingFixture";
    private const string IgnoredTestFixturePath = Net481TestSuitePath + ".IgnoredFixture";

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

    private NUnitTestRunnerProxy unit = null!;

    [SetUp]
    public void SetUp()
    {
        this.unit = new NUnitTestRunnerProxy();
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet481Assembly()
    {
        var testAssemblyDllPath = TestAssemblyNet481DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);

        // Then
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result, Has.One.Matches<TestSuiteEntity>(x => x.Name == "Net481"));

        var testSuite = result[0];
        Assert.That(testSuite.TestFixtures, Has.One.Matches<TestFixtureEntity>(x => x.Name == "SampleTestSuite"));

        var testFixture = testSuite.TestFixtures.Single(x => x.Name == "SampleTestSuite");
        Assert.That(testFixture.TestCases, Has.Length.EqualTo(4));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Pass"));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Fail"));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Error"));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Ignored"));
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet461Assembly()
    {
        var testAssemblyDllPath = TestAssemblyNet461DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);

        // Then
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithZatTestsAssembly()
    {
        var zatTestsAssemblyPath = Path.Combine(@"C:\Automized tests\Test libs\", "Zat.Z2xxTests.dll");

        if (!File.Exists(zatTestsAssemblyPath))
        {
            throw new FileNotFoundException(zatTestsAssemblyPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(zatTestsAssemblyPath);

        // Then
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task RunTestAsync__WhenRunWithAllTestCases__ThenShouldReturnTreeOfTestSuitesTestFixturesAndTestCases()
    {
        // Given:
        var testCases = GetTestCases(await this.LoadNet481TestAssemblyAsync());

        // When:
        var result = await this.unit.RunTestAsync(testCases);

        // Then:
        Assert.That(result.TestSuiteResults.Select(x => x.EntityName), Is.EqualTo(new[] { Net481TestSuitePath }));
        Assert.That(
            result.TestSuiteResults[0].TestFixtureResults.Select(x => x.EntityName),
            Is.EquivalentTo(new[]
            {
                SampleTestFixturePath,
                OneTimeSetUpFailingTestFixturePath,
                OneTimeTearDownFailingTestFixturePath,
                IgnoredTestFixturePath,
            }));
        Assert.That(
            GetTestCaseResults(result).Select(x => x.EntityName),
            Is.EquivalentTo(testCases.Select(x => x.ExecutionPath)));
    }

    [Test]
    public async Task RunTestAsync__WhenRunWithAllTestCases__ThenShouldReturnTestCaseIdsOfTestCaseEntities()
    {
        // Given:
        var testCases = GetTestCases(await this.LoadNet481TestAssemblyAsync());

        // When:
        var result = await this.unit.RunTestAsync(testCases);

        // Then:
        Assert.That(
            GetTestCaseResults(result).Select(x => (x.EntityName, x.Id)),
            Is.EquivalentTo(testCases.Select(x => (x.ExecutionPath, x.Id))));
    }

    [TestCase("Pass", TestStatus.Passed, null)]
    [TestCase("Fail", TestStatus.Failure, "FAILURE REASON")]
    [TestCase("Error", TestStatus.Error, "ERROR REASON")]
    [TestCase("Ignored", TestStatus.Ignored, "IGNORE REASON")]
    public async Task RunTestAsync__WhenRunWithSingleTestCase__ThenShouldReturnItsStatusAndDetail_AndPassedParents(
        string givenTestCaseName, TestStatus expectedStatus, string? expectedMessage)
    {
        // Given:
        var testCase = GetTestCases(await this.LoadNet481TestAssemblyAsync())
            .Single(x => x.ExecutionPath == $"{SampleTestFixturePath}.{givenTestCaseName}");

        // When:
        var result = await this.unit.RunTestAsync([testCase]);

        // Then:
        var testSuiteResult = result.TestSuiteResults.Single();
        Assert.That(testSuiteResult.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(testSuiteResult.Detail, Is.Null);

        var testFixtureResult = testSuiteResult.TestFixtureResults.Single();
        Assert.That(testFixtureResult.EntityName, Is.EqualTo(SampleTestFixturePath));
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(testFixtureResult.Detail, Is.Null);

        var testCaseResult = testFixtureResult.TestCaseResults.Single();
        Assert.That(testCaseResult.EntityName, Is.EqualTo(testCase.ExecutionPath));
        Assert.That(testCaseResult.Status, Is.EqualTo(expectedStatus));
        if (expectedMessage is null)
        {
            Assert.That(testCaseResult.Detail, Is.Null);
        }
        else
        {
            Assert.That(testCaseResult.Detail?.Message, Does.Contain(expectedMessage));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestFixtureOneTimeSetUpFails__ThenShouldReportErrorAtTestFixture_AndInheritItInTestCases()
    {
        // Given:
        var testCases = GetTestCases(await this.LoadNet481TestAssemblyAsync(), OneTimeSetUpFailingTestFixturePath);

        // When:
        var result = await this.unit.RunTestAsync(testCases);

        // Then:
        var testSuiteResult = result.TestSuiteResults.Single();
        Assert.That(testSuiteResult.Status, Is.EqualTo(TestStatus.Passed));

        var testFixtureResult = testSuiteResult.TestFixtureResults.Single();
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Error));
        Assert.That(testFixtureResult.Detail?.Message, Does.Contain("ONE TIME SETUP REASON"));
        Assert.That(testFixtureResult.Detail?.StackTrace, Is.Not.Empty);

        Assert.That(testFixtureResult.TestCaseResults, Has.Length.EqualTo(2));
        Assert.That(testFixtureResult.TestCaseResults, Has.All.Matches<TestCaseResult>(
            x => x.Status == TestStatus.Error && x.Detail!.Message.Contains("ONE TIME SETUP REASON")));
    }

    [Test]
    public async Task RunTestAsync__WhenTestFixtureOneTimeTearDownFails__ThenShouldReportErrorAtTestFixture_AndKeepTestCasesPassed()
    {
        // Given:
        var testCases = GetTestCases(await this.LoadNet481TestAssemblyAsync(), OneTimeTearDownFailingTestFixturePath);

        // When:
        var result = await this.unit.RunTestAsync(testCases);

        // Then:
        var testSuiteResult = result.TestSuiteResults.Single();
        Assert.That(testSuiteResult.Status, Is.EqualTo(TestStatus.Passed));

        var testFixtureResult = testSuiteResult.TestFixtureResults.Single();
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Error));
        Assert.That(testFixtureResult.Detail?.Message, Does.Contain("ONE TIME TEARDOWN REASON"));

        Assert.That(testFixtureResult.TestCaseResults, Has.Length.EqualTo(2));
        Assert.That(testFixtureResult.TestCaseResults, Has.All.Matches<TestCaseResult>(
            x => x.Status == TestStatus.Passed));
    }

    [Test]
    public async Task RunTestAsync__WhenTestFixtureIsIgnored__ThenShouldReportIgnoredAtTestFixture_AndInheritItInTestCases()
    {
        // Given:
        var testCases = GetTestCases(await this.LoadNet481TestAssemblyAsync(), IgnoredTestFixturePath);

        // When:
        var result = await this.unit.RunTestAsync(testCases);

        // Then:
        var testFixtureResult = result.TestSuiteResults.Single().TestFixtureResults.Single();
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Ignored));
        Assert.That(testFixtureResult.Detail?.Message, Does.Contain("FIXTURE IGNORE REASON"));

        Assert.That(testFixtureResult.TestCaseResults, Has.Length.EqualTo(2));
        Assert.That(testFixtureResult.TestCaseResults, Has.All.Matches<TestCaseResult>(
            x => x.Status == TestStatus.Ignored && x.Detail!.Message.Contains("FIXTURE IGNORE REASON")));
    }

    [Test]
    public async Task RunTestAsync__WhenRunWithTestSuiteEntity__ThenShouldReturnResultOfEveryTestCaseBeneathIt()
    {
        // Given:
        var testSuites = await this.LoadNet481TestAssemblyAsync();

        // When:
        var result = await this.unit.RunTestAsync([testSuites.Single()]);

        // Then:
        Assert.That(
            GetTestCaseResults(result).Select(x => x.EntityName),
            Is.EquivalentTo(GetTestCases(testSuites).Select(x => x.ExecutionPath)));
        Assert.That(GetTestCaseResults(result), Has.None.Matches<TestCaseResult>(x => x.Status == TestStatus.Unknown));
    }

    [Test]
    public async Task RunTestAsync__WhenRunWithTestFixtureEntity__ThenShouldReturnResultOfEveryTestCaseBeneathIt()
    {
        // Given:
        var testFixture = (await this.LoadNet481TestAssemblyAsync())
            .SelectMany(x => x.TestFixtures)
            .Single(x => x.ExecutionPath == SampleTestFixturePath);

        // When:
        var result = await this.unit.RunTestAsync([testFixture]);

        // Then:
        var testFixtureResult = result.TestSuiteResults.Single().TestFixtureResults.Single();
        Assert.That(testFixtureResult.EntityName, Is.EqualTo(SampleTestFixturePath));
        Assert.That(
            testFixtureResult.TestCaseResults.Select(x => x.EntityName),
            Is.EquivalentTo(testFixture.TestCases.Select(x => x.ExecutionPath)));
    }

    private static TestCaseEntity[] GetTestCases(TestSuiteEntity[] testSuites, string? testFixturePath = null)
        =>
        [
            .. testSuites
                .SelectMany(x => x.TestFixtures)
                .Where(x => testFixturePath is null || x.ExecutionPath == testFixturePath)
                .SelectMany(x => x.TestCases)
        ];

    private static TestCaseResult[] GetTestCaseResults(ProxyTestResult result)
        =>
        [
            .. result.TestSuiteResults
                .SelectMany(x => x.TestFixtureResults)
                .SelectMany(x => x.TestCaseResults)
        ];

    private Task<TestSuiteEntity[]> LoadNet481TestAssemblyAsync()
        => File.Exists(TestAssemblyNet481DllPath)
            ? this.unit.LoadTestAssemblyAsync(TestAssemblyNet481DllPath)
            : throw new FileNotFoundException(TestAssemblyNet481DllPath);
}