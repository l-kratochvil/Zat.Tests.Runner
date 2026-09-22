namespace Zat.Tests.Runner.Common.Net.Services;

public interface ITestLink
{
    Zat.Tests.Runner.Common.Net.TestLink.API.Model.Build[] GetBuildsForTestPlan(int testPlanId);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.TestSuite[] GetTestSuitesForTestSuite(int testSuiteId);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.TestSuite? GetTestSuiteById(int id);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.TestPlatform[] GetTestPlanPlatforms(int testPlanId);

    public Zat.Tests.Runner.Common.Net.TestLink.API.Model.GeneralResult UploadTestCaseExecutionResult(
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