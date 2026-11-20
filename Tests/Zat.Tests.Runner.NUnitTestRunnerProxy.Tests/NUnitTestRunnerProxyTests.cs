namespace Zat.Tests.Runner.NUnitTestRunnerProxy.Tests;

using System.Reflection;

using DevKit.Core.Extensions.Types;

using NUnit.Framework;

using NUnitTestAssembly.Common;

using Zat.Tests.Runner.Common.Model;

// TODO: Change to Net10 and communicate with proxy via StreamJsonRpc (as the production proxy client does) instead of directly calling the proxy implementation.
[TestFixture]
public class NUnitTestRunnerProxyTests
{
    private const string TestAssemblyNet461Name = NUnitTestAssembly.Net461.TestAssembly.Name;
    private const string TestAssemblyNet481Name = NUnitTestAssembly.Net481.TestAssembly.Name;

    private const string Net481TestSuitePath = TestAssemblyNet481Name;
    private const string NestedTestSuitePath = Net481TestSuitePath + ".Nested";
    private const string TestCaseFixturePath = Net481TestSuitePath + "." + nameof(NUnitTestAssembly.Net481.TestCaseFixture);
    private const string OneTimeSetUpFailingTestFixturePath = Net481TestSuitePath + "." + nameof(NUnitTestAssembly.Net481.OneTimeSetUpFailingFixture);
    private const string OneTimeTearDownFailingTestFixturePath = Net481TestSuitePath + "." + nameof(NUnitTestAssembly.Net481.OneTimeTearDownFailingFixture);
    private const string IgnoredTestFixturePath = Net481TestSuitePath + "." + nameof(NUnitTestAssembly.Net481.IgnoredFixture);
    private const string TearDownAttachmentTestFixturePath = Net481TestSuitePath + "." + nameof(NUnitTestAssembly.Net481.TearDownAttachmentFixture);
    private const string NestedTestFixturePath = NestedTestSuitePath + "." + nameof(NUnitTestAssembly.Net481.Nested.NestedFixture);

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

    [Test]
    public async Task DiscoverTestsAsync__WhenCalledWithNet481Assembly__ThenShouldReturnTreeOfTestSuitesTestFixturesAndTestCases()
    {
        // Given:
        var testAssemblyDllPath = EnsureFileExists(TestAssemblyNet481DllPath);
        var unit = CreateUnit();

        // When:
        var result = await unit.DiscoverTestsAsync(testAssemblyDllPath);

        // Then:
        Assert.That(
            result.Select(x => x.Name),
            Is.EquivalentTo([nameof(NUnitTestAssembly.Net481), nameof(NUnitTestAssembly.Net481.Nested)]));

        var testSuite = result.Single(x => x.Name == nameof(NUnitTestAssembly.Net481));
        Assert.That(testSuite.TestFixtures.Select(x => x.Name), Is.EquivalentTo(
        [
            nameof(NUnitTestAssembly.Net481.TestCaseFixture),
            nameof(NUnitTestAssembly.Net481.OneTimeSetUpFailingFixture),
            nameof(NUnitTestAssembly.Net481.OneTimeTearDownFailingFixture),
            nameof(NUnitTestAssembly.Net481.IgnoredFixture),
            nameof(NUnitTestAssembly.Net481.TearDownAttachmentFixture),
        ]));

        var testFixture = testSuite.TestFixtures.Single(x => x.Name == nameof(NUnitTestAssembly.Net481.TestCaseFixture));
        Assert.That(
            testFixture.TestCases.Select(x => x.Name),
            Is.EquivalentTo(
            [
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest),
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.FailingTest),
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.ThrowingTest),
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.IgnoredTest),
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.AttachingTest),
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.UseConfigurationFileTest),
                nameof(NUnitTestAssembly.Net481.TestCaseFixture.UseTestRunnerBridgeTest),
            ]));

        var nestedTestSuite = result.Single(x => x.Name == nameof(NUnitTestAssembly.Net481.Nested));
        var nestedTestFixture = nestedTestSuite.TestFixtures.Single();
        Assert.That(
            nestedTestFixture.Name,
            Is.EqualTo(nameof(NUnitTestAssembly.Net481.Nested.NestedFixture)));
        Assert.That(
            nestedTestFixture.TestCases.Select(x => x.Name),
            Is.EqualTo([nameof(NUnitTestAssembly.Net481.Nested.NestedFixture.NestedTest)]));
    }

    [Test]
    public async Task DiscoverTestsAsync__WhenCalledWithNet461Assembly__ThenShouldReturnTreeOfTestSuitesTestFixturesAndTestCases()
    {
        // Given:
        var testAssemblyDllPath = EnsureFileExists(TestAssemblyNet461DllPath);
        var unit = CreateUnit();

        // When:
        var result = await unit.DiscoverTestsAsync(testAssemblyDllPath);

        // Then:
        Assert.That(result.Select(x => x.Name), Is.EqualTo([nameof(NUnitTestAssembly.Net461)]));
        Assert.That(
            result[0].TestFixtures.Select(x => x.Name),
            Is.EqualTo([nameof(NUnitTestAssembly.Net461.TestCaseFixture)]));
        Assert.That(
            result[0].TestFixtures[0].TestCases.Select(x => x.Name),
            Is.EquivalentTo(
            [
                nameof(NUnitTestAssembly.Net461.TestCaseFixture.PassingTest),
                nameof(NUnitTestAssembly.Net461.TestCaseFixture.FailingTest),
                nameof(NUnitTestAssembly.Net461.TestCaseFixture.ThrowingTest),
                nameof(NUnitTestAssembly.Net461.TestCaseFixture.IgnoredTest),
                nameof(NUnitTestAssembly.Net461.TestCaseFixture.AttachingTest),
            ]));
    }

    [Test]
    public async Task DiscoverTestsAsync__WhenCalledWithZatTestsAssembly__ThenShouldReturnTestSuites()
    {
        // Given:
        var zatTestsAssemblyPath = EnsureFileExists(Path.Combine(@"C:\Automized tests\Test libs\", "Zat.Z2xxTests.dll"));
        var unit = CreateUnit();

        // When:
        var result = await unit.DiscoverTestsAsync(zatTestsAssemblyPath);

        // Then:
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task RunTestsAsync__WhenRunWithAllTestCases__ThenShouldReturnTreeOfTestSuitesTestFixturesAndTestCases()
    {
        // Given:
        var unit = CreateUnit();
        var testCases = GetTestCases(await DiscoverNet481TestsAsync(unit));

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testCases);

        // Then:
        Assert.That(
            result.TestSuiteResults.Select(x => x.EntityName),
            Is.EquivalentTo([Net481TestSuitePath, NestedTestSuitePath]));
        Assert.That(
            result.TestSuiteResults.Single(x => x.EntityName == Net481TestSuitePath).TestFixtureResults.Select(x => x.EntityName),
            Is.EquivalentTo(
            [
                TestCaseFixturePath,
                OneTimeSetUpFailingTestFixturePath,
                OneTimeTearDownFailingTestFixturePath,
                IgnoredTestFixturePath,
                TearDownAttachmentTestFixturePath,
            ]));
        Assert.That(
            result.TestSuiteResults.Single(x => x.EntityName == NestedTestSuitePath).TestFixtureResults.Select(x => x.EntityName),
            Is.EqualTo([NestedTestFixturePath]));
        Assert.That(
            GetTestCaseResults(result).Select(x => x.EntityName),
            Is.EquivalentTo(testCases.Select(x => x.ExecutionPath)));
    }

    [Test]
    public async Task RunTestsAsync__WhenRunWithAllTestCases__ThenShouldReturnTestCaseIdsOfTestCaseEntities()
    {
        // Given:
        var unit = CreateUnit();
        var testCases = GetTestCases(await DiscoverNet481TestsAsync(unit));

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testCases);

        // Then:
        Assert.That(
            GetTestCaseResults(result).Select(x => (x.EntityName, x.Id)),
            Is.EquivalentTo(testCases.Select(x => (x.ExecutionPath, x.Id))));
    }

    [TestCase(nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest), TestStatus.Passed, null)]
    [TestCase(nameof(NUnitTestAssembly.Net481.TestCaseFixture.FailingTest), TestStatus.Failure, NUnitTestAssembly.Net481.TestCaseFixture.FailureReason)]
    [TestCase(nameof(NUnitTestAssembly.Net481.TestCaseFixture.ThrowingTest), TestStatus.Error, NUnitTestAssembly.Net481.TestCaseFixture.ErrorReason)]
    [TestCase(nameof(NUnitTestAssembly.Net481.TestCaseFixture.IgnoredTest), TestStatus.Ignored, NUnitTestAssembly.Net481.TestCaseFixture.IgnoreReason)]
    public async Task RunTestsAsync__WhenRunWithSingleTestCase__ThenShouldReturnItsStatusAndDetail_AndPassedParents(
        string givenTestCaseName, TestStatus expectedStatus, string? expectedMessage)
    {
        // Given:
        var unit = CreateUnit();
        var testCase = GetTestCases(await DiscoverNet481TestsAsync(unit))
            .Single(x => x.ExecutionPath == $"{TestCaseFixturePath}.{givenTestCaseName}");

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, [testCase]);

        // Then:
        var testSuiteResult = result.TestSuiteResults.Single();
        Assert.That(testSuiteResult.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(testSuiteResult.Detail, Is.Null);

        var testFixtureResult = testSuiteResult.TestFixtureResults.Single();
        Assert.That(testFixtureResult.EntityName, Is.EqualTo(TestCaseFixturePath));
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
    public async Task RunTestsAsync__WhenTestFixtureOneTimeSetUpFails__ThenShouldReportErrorAtTestFixture_AndInheritItInTestCases()
    {
        // Given:
        var unit = CreateUnit();
        var testCases = GetTestCases(await DiscoverNet481TestsAsync(unit), OneTimeSetUpFailingTestFixturePath);

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testCases);

        // Then:
        var testSuiteResult = result.TestSuiteResults.Single();
        Assert.That(testSuiteResult.Status, Is.EqualTo(TestStatus.Passed));

        var testFixtureResult = testSuiteResult.TestFixtureResults.Single();
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Error));
        Assert.That(testFixtureResult.Detail?.Message, Does.Contain(NUnitTestAssembly.Net481.OneTimeSetUpFailingFixture.ErrorReason));
        Assert.That(testFixtureResult.Detail?.StackTrace, Is.Not.Empty);

        Assert.That(testFixtureResult.TestCaseResults, Has.Length.EqualTo(2));
        Assert.That(testFixtureResult.TestCaseResults, Has.All.Matches<TestCaseResult>(
            x => x.Status == TestStatus.Error && x.Detail!.Message.Contains(NUnitTestAssembly.Net481.OneTimeSetUpFailingFixture.ErrorReason)));
    }

    [Test]
    public async Task RunTestsAsync__WhenTestFixtureOneTimeTearDownFails__ThenShouldReportErrorAtTestFixture_AndKeepTestCasesPassed()
    {
        // Given:
        var unit = CreateUnit();
        var testCases = GetTestCases(await DiscoverNet481TestsAsync(unit), OneTimeTearDownFailingTestFixturePath);

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testCases);

        // Then:
        var testSuiteResult = result.TestSuiteResults.Single();
        Assert.That(testSuiteResult.Status, Is.EqualTo(TestStatus.Passed));

        var testFixtureResult = testSuiteResult.TestFixtureResults.Single();
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Error));
        Assert.That(testFixtureResult.Detail?.Message, Does.Contain(NUnitTestAssembly.Net481.OneTimeTearDownFailingFixture.ErrorReason));

        Assert.That(testFixtureResult.TestCaseResults, Has.Length.EqualTo(2));
        Assert.That(testFixtureResult.TestCaseResults, Has.All.Matches<TestCaseResult>(
            x => x.Status == TestStatus.Passed));
    }

    [Test]
    public async Task RunTestsAsync__WhenTestFixtureIsIgnored__ThenShouldReportIgnoredAtTestFixture_AndInheritItInTestCases()
    {
        // Given:
        var unit = CreateUnit();
        var testCases = GetTestCases(await DiscoverNet481TestsAsync(unit), IgnoredTestFixturePath);

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testCases);

        // Then:
        var testFixtureResult = result.TestSuiteResults.Single().TestFixtureResults.Single();
        Assert.That(testFixtureResult.Status, Is.EqualTo(TestStatus.Ignored));
        Assert.That(testFixtureResult.Detail?.Message, Does.Contain(NUnitTestAssembly.Net481.IgnoredFixture.IgnoreReason));

        Assert.That(testFixtureResult.TestCaseResults, Has.Length.EqualTo(2));
        Assert.That(testFixtureResult.TestCaseResults, Has.All.Matches<TestCaseResult>(
            x => x.Status == TestStatus.Ignored && x.Detail!.Message.Contains(NUnitTestAssembly.Net481.IgnoredFixture.IgnoreReason)));
    }

    [Test]
    public async Task RunTestsAsync__WhenRunWithTestSuiteEntity__ThenShouldReturnResultOfEveryTestCaseBeneathIt()
    {
        // Given:
        var unit = CreateUnit();
        var testSuites = await DiscoverNet481TestsAsync(unit);

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testSuites);

        // Then:
        Assert.That(
            GetTestCaseResults(result).Select(x => x.EntityName),
            Is.EquivalentTo(GetTestCases(testSuites).Select(x => x.ExecutionPath)));
        Assert.That(GetTestCaseResults(result), Has.None.Matches<TestCaseResult>(x => x.Status == TestStatus.Unknown));
    }

    [Test]
    public async Task RunTestsAsync__WhenRunWithTestFixtureEntity__ThenShouldReturnResultOfEveryTestCaseBeneathIt()
    {
        // Given:
        var unit = CreateUnit();
        var testFixture = (await DiscoverNet481TestsAsync(unit))
            .SelectMany(x => x.TestFixtures)
            .Single(x => x.ExecutionPath == TestCaseFixturePath);

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, [testFixture]);

        // Then:
        var testFixtureResult = result.TestSuiteResults.Single().TestFixtureResults.Single();
        Assert.That(testFixtureResult.EntityName, Is.EqualTo(TestCaseFixturePath));
        Assert.That(
            testFixtureResult.TestCaseResults.Select(x => x.EntityName),
            Is.EquivalentTo(testFixture.TestCases.Select(x => x.ExecutionPath)));
    }

    [Test]
    public async Task RunTestsAsync__WhenTestAssemblyHasConfigurationFile__ThenShouldApplyItToTestCases()
    {
        // Given:
        var unit = CreateUnit();
        var testCases = GetTestCases(await DiscoverNet481TestsAsync(unit), TestCaseFixturePath)
            .Where(x => x.Name == nameof(NUnitTestAssembly.Net481.TestCaseFixture.UseConfigurationFileTest));

        // When:
        var result = await unit.RunTestsAsync(TestAssemblyNet481DllPath, testCases);

        // Then:
        var testCaseResult = GetTestCaseResults(result).Single();
        Assert.That(testCaseResult.Status, Is.EqualTo(TestStatus.Passed), testCaseResult.Detail?.Message);
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

    private static Task<TestSuiteEntity[]> DiscoverNet481TestsAsync(NUnitTestRunnerProxy unit)
        => unit.DiscoverTestsAsync(EnsureFileExists(TestAssemblyNet481DllPath));

    private static string EnsureFileExists(string filePath)
        => File.Exists(filePath) ? filePath : throw new FileNotFoundException(filePath);

    private static NUnitTestRunnerProxy CreateUnit()
        => new();
}