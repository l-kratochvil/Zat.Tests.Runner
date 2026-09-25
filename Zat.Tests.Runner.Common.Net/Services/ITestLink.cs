namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

public interface ITestLink
{
    Build[] GetBuildsForTestPlan(int testPlanId);

    GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    TestCase GetTestCaseById(int id);

    TestCase GetTestCaseByExternalId(string externalId);

    GeneralResult ReportTestCaseResult(
        int testCaseId,
        int testPlanId,
        string status,
        int platformId = 0,
        string? platformName = null,
        bool overwrite = false,
        bool guess = true,
        string notes = "",
        int buildId = 0,
        int bugId = 0);

    AttachmentRequestResponse UploadExecutionAttachment(
        int executionId,
        string filename,
        string fileType,
        byte[] content,
        string title = "",
        string description = "");

    // TODO: Is this needed?
    TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId);

    // TODO: Is this needed?
    TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep);

    // TODO: Is this needed?
    TestSuite[] GetTestSuitesForTestSuite(int testSuiteId);

    // TODO: Is this needed?
    TestSuite? GetTestSuiteById(int id);

    // TODO: Is this needed?
    TestPlatform[] GetTestPlanPlatforms(int testPlanId);

    public record Config(
        string ApiKey,
        string XmlRpcServerUrl,
        bool LoggingEnabled)
    {
        public static Config Default
            => new(
                ApiKey: "dc7a17e14a9f1879d38583a38c3a81e8",
                XmlRpcServerUrl: "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php",
                LoggingEnabled: false);
    }
}