namespace Zat.Tests.Runner.Common.Net.Services;

using System.Diagnostics;

using DevKit.Core.Extensions.Types;

using Zat.Tests.Runner.Common.Net;

public class TestLinkResultHandler(
    ITestLink testLink,
    TestLinkResultHandler.IContext context)
    : ITestResultHandler
{
    /// <inheritdoc />
    public void Handle(TestResult testResult)
    {
        Debug.SafeFail("TODO");

        var testPlatform = testLink.GetTestPlanPlatforms(result.testPlanId).First();

        if (!testLink.GetBuildsForTestPlan(result.testPlanId).Any(x => x.name == build))
        {
            testLink.CreateBuild(result.testPlanId, build, string.Empty);
        }

        var testBuild = testLink.GetBuildsForTestPlan(result.testPlanId).First(x => x.name == build);

        // NOTE: testcase/testsuite ID se získá: Specifikace testů >> pravé tl. myši na test. příp. ve stromu
        var testsuiteTestcases =
            testLink.GetTestCasesForTestSuite(result.testSuiteId, true);

        var
            testcase = testsuiteTestcases.First(x
                => x.external_id == result.testCaseId.ToString()); // 44 je číselná složka z ID ve formátu Z200-XX (Z200-44)
        var testcaseApiId = testcase.id;

        var resultStatus = result.status switch
        {
            TestStatus.Passed => "p",
            TestStatus.Failed => "f",
            TestStatus.Skipped => "b",
            _ => string.Empty,
        };

        var res = testLink.UploadTestCaseExecutionResult(
            testcaseApiId,
            result.testPlanId,
            resultStatus,
            platformId: testPlatform.id, // Platforma musí být přidána do testovacího plánu. Pokud není potřeba specifikovat platformu, tak stačí zadat prázdný string do argument platfromName
            overwrite: false,
            notes: result.notes,
            buildId: testBuild.id);
    }

    public interface IContext
    {
        string? IdeVersion { get; }

        string? IdeReleaseDate { get; }

        string? RuntimeVersion { get; }

        string? RuntimeReleaseDate { get; }
    }
}