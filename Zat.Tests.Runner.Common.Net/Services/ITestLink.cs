namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Net.TestLink.API.Model;

public interface ITestLink
{
    Build[] GetBuildsForTestPlan(int testPlanId);

    GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId);

    TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep);

    TestSuite[] GetTestSuitesForTestSuite(int testSuiteId);

    TestSuite? GetTestSuiteById(int id);

    TestPlatform[] GetTestPlanPlatforms(int testPlanId);

    GeneralResult UploadTestCaseExecutionResult(
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