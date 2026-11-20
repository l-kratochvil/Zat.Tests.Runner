namespace Zat.Tests.Runner.Common.Net.Services;

using System.Net;

using CookComputing.XmlRpc;

using Microsoft.Extensions.Options;

using Zat.Tests.Runner.Common.Net.Application;
using Zat.Tests.Runner.Common.Net.TestLinkApi;
using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

// TODO: Review method summaries
public class TestLink : ITestLink
{
    private readonly string devKey;

    private readonly ITestLinkXmlRpcProxy proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestLink"/> class.
    /// </summary>
    public TestLink(
        ITestLink.Config config,
        IOptions<TestLinkSecrets> testLinkSecrets)
    {
        var apiKey = testLinkSecrets.Value.ApiKey;
        var xmlRpcServerUrl = config.XmlRpcServerUrl;

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new TestLinkApiException("API key wasn't provided");
        }

        this.devKey = apiKey;

        if (string.IsNullOrEmpty(xmlRpcServerUrl))
        {
            throw new TestLinkApiException(
                $"TestLink XML RPC server URL wasn't provided. Provided dev key: {this.devKey}");
        }

        this.proxy = XmlRpcProxyGen.Create<ITestLinkXmlRpcProxy>();
        this.proxy.Url = xmlRpcServerUrl;
        ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
    }

    /// <inheritdoc />
    public string SayHello()
        => this.proxy.SayHello();

    /// <summary>
    /// Gets a list of all builds for a testplan.
    /// </summary>
    /// <param name="testPlanId">The id of the testplan.</param>
    /// <returns>The builds.</returns>
    public Build[] GetBuildsForTestPlan(int testPlanId)
    {
        var response = this.proxy.GetBuildsForTestPlan(this.devKey, testPlanId);

        CheckErrorMessage(response);

        if (response is string responseText && string.IsNullOrEmpty(responseText))
        {
            return [];
        }

        return
        [
            .. ((object[])response)
            .Cast<XmlRpcStruct>()
            .Select(XmlRpcStructConvertors.ToBuild)
        ];
    }

    /// <summary>
    /// create a build for a testplan.
    /// </summary>
    /// <param name="testPlanId">id of the test plan.</param>
    /// <param name="buildName">name of the build.</param>
    /// <param name="buildNotes">notes.</param>
    /// <returns>General result.</returns>
    public GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes)
    {
        var response = this.proxy.CreateBuild(this.devKey, testPlanId, buildName, buildNotes);

        CheckErrorMessage(response);

        return XmlRpcStructConvertors.ToGeneralResult((XmlRpcStruct)response[0]);
    }

    /// <summary>
    /// Uploads the result of a test case execution.
    /// </summary>
    /// <param name="testCaseId">Id of test case.</param>
    /// <param name="testplanid">Id of test plan.</param>
    /// <param name="status">The result of the test (pass: p, fail: f or blocked: b).</param>
    /// <param name="platformId">Id of the platform. Optional if platform name is given.</param>
    /// <param name="platformName">name of the platform. Optional if the platform id is given.</param>
    /// <param name="overwrite">if true, then last execution for (testcase,testplan,build,platform) will be overwritten.</param>
    /// <param name="guess"> (assumed to be true) defining whether to guess optinal params or require them explicitly default is true.</param>
    /// <param name="notes">any notes or info to be added to the description field.</param>
    /// <param name="buildId">If not given, then highest build id willl be used.</param>
    /// <param name="bugId">Id for a bug if used in conjunction with a defect tracker.</param>
    /// <returns>General result.</returns>
    public GeneralResult ReportTestCaseResult(
        int testCaseId,
        int testplanid,
        string status,
        int platformId = 0,
        string? platformName = null,
        bool overwrite = false,
        bool guess = true,
        string notes = "",
        int buildId = 0,
        int bugId = 0)
    {
        object GetResponse()
        {
            if (platformName is not null)
            {
                if (bugId == 0)
                {
                    return buildId == 0
                        ? this.proxy.ReportTcResult(
                            this.devKey, testCaseId, testplanid, status, platformName, overwrite, notes, guess)
                        : this.proxy.ReportTcResult(
                            this.devKey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, 0, buildId);
                }

                return buildId == 0
                    ? this.proxy.ReportTcResult(
                        this.devKey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, bugId)
                    : this.proxy.ReportTcResult(
                        this.devKey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, bugId, buildId);
            }

            if (platformId == 0)
            {
                throw new TestLinkApiException("Must supply either a platform id or a platform name");
            }

            if (bugId == 0)
            {
                return buildId == 0
                    ? this.proxy.ReportTcResult(this.devKey, testCaseId, testplanid, status, platformId, overwrite, notes, guess)
                    : this.proxy.ReportTcResult(this.devKey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, 0, buildId);
            }

            return buildId == 0
                ? this.proxy.ReportTcResult(this.devKey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, bugId)
                : this.proxy.ReportTcResult(this.devKey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, bugId, buildId);
        }

        var response = GetResponse();

        CheckErrorMessage(response);

        if (response is not object[] { Length: > 0 } responseList)
        {
            return new GeneralResult();
        }

        var msg = (XmlRpcStruct)responseList[0];
        var result = XmlRpcStructConvertors.ToGeneralResult(msg);

        return result;
    }

    /// <inheritdoc />
    public AttachmentRequestResponse UploadExecutionAttachment(
        int executionId,
        string filename,
        string fileType,
        byte[] content,
        string title = "",
        string description = "")
    {
        string base64String;
        try
        {
            base64String = Convert.ToBase64String(content, 0, content.Length);
        }
        catch (ArgumentNullException)
        {
            base64String = string.Empty;
        }

        var response = this.proxy.UploadExecutionAttachment(this.devKey, executionId, filename, fileType, base64String, title, description);

        CheckErrorMessage(response);

        return XmlRpcStructConvertors.ToAttachmentRequestResponse((XmlRpcStruct)response);
    }

    /// <inheritdoc />
    public TestCase GetTestCaseByExternalId(string externalId)
    {
        var response = this.proxy.GetTestCaseByExternalId(this.devKey, externalId);

        CheckErrorMessage(response);

        return XmlRpcStructConvertors.ToTestCase((XmlRpcStruct)((object[])response).First());
    }

    /// <inheritdoc />
    public TestCaseFromTestPlan[] GetTestCasesForTestPlan(int testPlanId)
    {
        var response = this.proxy.GetTestCasesForTestPlan(this.devKey, testPlanId);

        CheckErrorMessage(response);

        // Keyed by test case ID; each value holds one entry per platform.
        if (response is not XmlRpcStruct testCases)
        {
            return [];
        }

        return
        [
            .. testCases.Values
                .OfType<object[]>()
                .Select(platformEntries => platformEntries.OfType<XmlRpcStruct>().FirstOrDefault())
                .OfType<XmlRpcStruct>()
                .Select(XmlRpcStructConvertors.ToTestCaseFromTestPlan)
        ];
    }

    /// <summary>
    /// Processes <paramref name="response"/> returned by the TestLink API for error messages.
    /// </summary>
    /// <param name="response">The response returned by TestLink.</param>
    /// <exception cref="TestLinkApiException">Thrown if <paramref name="response"/> contains an error message.</exception>
    private static void CheckErrorMessage(object response)
    {
        if (response is not object[] errorMessages)
        {
            return;
        }

        var error = DecodeErrors(errorMessages).FirstOrDefault();
        if (error is not null)
        {
            throw new TestLinkApiException(
                $"TestLink API returned error '{error.Code}'. Error message: {error.Message}");
        }
    }

    private static List<TestLinkErrorMessage> DecodeErrors(object[] messages)
        =>
        [
            .. messages
                .Cast<XmlRpcStruct>()
                .Where(message => message.ContainsKey("code") && message.ContainsKey("message"))
                .Select(XmlRpcStructConvertors.ToTestLinkErrorMessage)
        ];
}