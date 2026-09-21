namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

public interface ITestLinkApiClient
{
    Build[] GetBuildsForTestPlan(int testPlanId);

    public GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    public TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId);

    public TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep);

    public TestSuite[] GetTestSuitesForTestSuite(int testSuiteId);

    public TestSuite? GetTestSuiteById(int id);

    public TestPlatform[] GetTestPlanPlatforms(int testPlanId);

    public GeneralResult UploadTestCaseExecutionResult(
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