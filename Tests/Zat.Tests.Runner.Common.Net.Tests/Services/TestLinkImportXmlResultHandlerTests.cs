namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using System.Xml.Linq;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Z2xxTests.Common.Model;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestLinkImportXmlResultHandlerTests
{
    private static readonly DateTimeOffset StartTime = new(2026, 10, 13, 8, 0, 0, TimeSpan.Zero);

    private string tempDirPath = null!;
    private TestLinkImportXmlResultHandler unit = null!;

    [SetUp]
    public void SetUp()
    {
        this.tempDirPath = Directory.CreateTempSubdirectory().FullName;
        this.unit = new TestLinkImportXmlResultHandler(
            NullLogger<LogSources.TestResultHandler>.Instance,
            this.tempDirPath);
    }

    [TearDown]
    public void TearDown()
        => Directory.Delete(this.tempDirPath, recursive: true);

    [Test]
    public void HandleSubRunResult__WhenApplicationAndRuntimeSubRunsEnd__ThenShouldSaveTestLinkImportXmlForEach()
    {
        // When:
        this.unit.HandleSubRunResult(SubRun(
            TestType.Application,
            hwAssemblyType: null,
            new TestCaseResult("TC-1", "Suite.Fixture.TC-1", TestStatus.Passed)));
        this.unit.HandleSubRunResult(SubRun(
            TestType.Runtime,
            HwAssemblyType.HW02_BB1M,
            new TestCaseResult("TC-2", "Suite.Fixture.TC-2", TestStatus.Failure, new Detail("Boom", "at X"))));

        // Then:
        var date = StartTime.LocalDateTime.ToString("ddMMyyyy");
        var filePaths = Directory.GetFiles(this.tempDirPath).Order(StringComparer.Ordinal).ToArray();
        Assert.That(
            filePaths.Select(Path.GetFileName),
            Is.EqualTo(new[]
            {
                $"TEST-RESULT_{date}_APPLICATIONS ({TestRunIdOf(filePaths[0])}).xml",
                $"TEST-RESULT_HW02_BB1M_{date}_RUNTIME ({TestRunIdOf(filePaths[0])}).xml",
            }));
        Assert.That(
            filePaths.Select(ReadTestCases),
            Is.EqualTo(new[]
            {
                new[] { ("TC-1", "p") },
                new[] { ("TC-2", "f") },
            }));
    }

    private static string TestRunIdOf(string filePath)
        => filePath[(filePath.LastIndexOf('(') + 1)..filePath.LastIndexOf(')')];

    private static (string ExternalId, string Result)[] ReadTestCases(string filePath)
        =>
        [
            .. XDocument
                .Load(filePath)
                .Descendants("testcase")
                .Select(x => ((string)x.Attribute("external_id")!, (string)x.Element("result")!)),
        ];

    private static TestResult SubRun(
        TestType testType, HwAssemblyType? hwAssemblyType, params TestCaseResult[] testCaseResults)
        => new(
            new ProxyTestResult(
            [
                new TestSuiteResult(
                    [new TestFixtureResult(testCaseResults, "Fixture", TestStatus.Passed)],
                    "Suite",
                    TestStatus.Passed),
            ]),
            testType,
            StartTime,
            StartTime,
            hwAssemblyType);
}