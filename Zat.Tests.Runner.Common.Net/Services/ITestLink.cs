namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

public interface ITestLink
{
    /// <summary>
    /// Executes basic ping against the TestLink XML-RPC server.
    /// </summary>
    /// <returns>The greeting returned by TestLink.</returns>
    string SayHello();

    Build[] GetBuildsForTestPlan(int testPlanId);

    GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    TestCase GetTestCaseByExternalId(string externalId);

    TestCaseFromTestPlan[] GetTestCasesForTestPlan(int testPlanId);

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

    public record Config(string XmlRpcServerUrl)
    {
        public static Config Default
            => new(XmlRpcServerUrl: "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php");
    }
}