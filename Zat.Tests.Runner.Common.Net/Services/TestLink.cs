namespace Zat.Tests.Runner.Common.Net.Services;

using System.Net;

using CookComputing.XmlRpc;

using Zat.Tests.Runner.Common.Net.TestLink.API;
using Zat.Tests.Runner.Common.Net.TestLink.API.Model;

// TODO: Review method summaries
public class TestLink : ITestLink
{
    private readonly string devkey;

    private readonly ITestLinkXmlRpcProxy proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestLink"/> class.
    /// </summary>
    /// <param name="apiKey">TestLink API key as provided by testlink.</param>
    /// <param name="xmlRpcServerUrl">URL of testlink XML RPC server. Something like: http://localhost/testlink/lib/api/xmlrpc.php</param>
    /// <param name="loggingEnabled">Enable capture of lastRequest and lastResponse for debugging.</param>
    public TestLink(ITestLink.Config config)
    {
        var apiKey = config.ApiKey;
        var xmlRpcServerUrl = config.XmlRpcServerUrl;
        var loggingEnabled = config.LoggingEnabled;

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new TestLinkApiException(
                $"API key wasn't provided. Provided devkey: {apiKey}");
        }

        this.devkey = apiKey;

        if (string.IsNullOrEmpty(xmlRpcServerUrl))
        {
            throw new TestLinkApiException(
                $"TestLink XML RPC server URL wasn't provided. Provided devkey: {this.devkey}");
        }

        this.proxy = XmlRpcProxyGen.Create<ITestLinkXmlRpcProxy>();
        this.proxy.Url = xmlRpcServerUrl;
        ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;

        if (!loggingEnabled)
        {
            return;
        }

        this.proxy.RequestEvent += this.HandleRequestEvent;
        this.proxy.ResponseEvent += this.HandleResponseEvent;
    }

    /// <summary>
    /// Gets the last xmlrpc request sent to testlink. Only works when loggingEnabled=true the constructor.
    /// </summary>
    public string LastDebugRequest { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the last xmlrpc response received from testlink. Only works when loggingEnabled=true in the constructor.
    /// </summary>
    public string LastDebugResponse { get; private set; } = string.Empty;

    /// <summary>
    /// Executes basic ping.
    /// </summary>
    /// <returns></returns>
    public string SayHello()
        => this.proxy.SayHello();

    /// <summary>
    /// Gets info about the API.
    /// </summary>
    /// <returns></returns>
    public string About()
        => this.proxy.About();

    /// <summary>
    /// Gets a list of all builds for a testplan.
    /// </summary>
    /// <param name="testPlanId">The id of the testplan.</param>
    /// <returns>The builds.</returns>
    public Build[] GetBuildsForTestPlan(int testPlanId)
    {
        var response = this.proxy.GetBuildsForTestPlan(this.devkey, testPlanId);

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
        var response = this.proxy.CreateBuild(this.devkey, testPlanId, buildName, buildNotes);

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
                            this.devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess)
                        : this.proxy.ReportTcResult(
                            this.devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, 0, buildId);
                }

                return buildId == 0
                    ? this.proxy.ReportTcResult(
                        this.devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, bugId)
                    : this.proxy.ReportTcResult(
                        this.devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, bugId, buildId);
            }

            if (platformId == 0)
            {
                throw new TestLinkApiException("Must supply either a platform id or a platform name");
            }

            if (bugId == 0)
            {
                return buildId == 0
                    ? this.proxy.ReportTcResult(this.devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess)
                    : this.proxy.ReportTcResult(this.devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, 0, buildId);
            }

            return buildId == 0
                ? this.proxy.ReportTcResult(this.devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, bugId)
                : this.proxy.ReportTcResult(this.devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, bugId, buildId);
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

    /// <summary>
    /// Uploads an attachment for an execution.
    /// </summary>
    /// <remarks>The attachment content must be Base64 encoded by the client before sending it.</remarks>
    /// <param name="executionId"></param>
    /// <param name="title">The title of the Attachment.</param>
    /// <param name="description">The description of the Attachment.</param>
    /// <param name="filename">The file name of the Attachment (e.g.: notes.txt).</param>
    /// <param name="fileType">The file type of the Attachment (e.g.: text/plain).</param>
    /// <param name="content">The content (Base64 encoded) of the Attachment.</param>
    /// <returns>Attachment request response.</returns>
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

        var response = this.proxy.UploadExecutionAttachment(this.devkey, executionId, filename, fileType, base64String, title, description);

        CheckErrorMessage(response);

        return XmlRpcStructConvertors.ToAttachmentRequestResponse((XmlRpcStruct)response);
    }

    public TestCase GetTestCaseById(int id)
    {
        var response = this.proxy.GetTestCaseById(this.devkey, id);

        CheckErrorMessage(response);

        return XmlRpcStructConvertors.ToTestCase((XmlRpcStruct)response);
    }

    /// <summary>
    /// Gets a test case by its external id.
    /// </summary>
    /// <param name="externalId">External ID including the prefix.</param>
    /// <returns>The test case.</returns>
    public TestCase GetTestCaseByExternalId(string externalId)
    {
        var response = this.proxy.GetTestCaseByExternalId(this.devkey, externalId);

        CheckErrorMessage(response);

        var singleData = ((object[])response).First();

        return XmlRpcStructConvertors.ToTestCase((XmlRpcStruct)singleData);
    }

    /// <summary>
    /// Gets test cases contained in a test suite.
    /// </summary>
    /// <param name="testSuiteId">Id of the test suite.</param>
    /// <param name="deep">Set the deep flag to false if you only want test cases in the test suite provided and no child test cases.</param>
    /// <returns>The testcases.</returns>
    public TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep)
    {
        var response = this.proxy.GetTestCasesForTestSuite(this.devkey, testSuiteId, deep, "full");
        if (response is string str && str == string.Empty)
        {
            return [];
        }

        CheckErrorMessage(response);

        return
        [
            .. ((object[])response)
            .Cast<XmlRpcStruct>()
            .Select(XmlRpcStructConvertors.ToTestCaseFromTestSuite)
        ];
    }

    /// <summary>
    /// Gets a list of all platforms for a test plan.
    /// </summary>
    /// <remarks>Throws an exception of type Testlink Exception.</remarks>
    /// <param name="testplanid">Id of the test plan.</param>
    /// <returns>The test platforms.</returns>
    public TestPlatform[] GetTestPlanPlatforms(int testplanid)
    {
        var response = this.proxy.GetTestPlanPlatforms(this.devkey, testplanid);

        // 3041 means no platforms are assigned for this testplan
        return CheckErrorMessage(response, 3041)
            ? []
            :
            [
                .. ((object[])response)
                .Cast<XmlRpcStruct>()
                .Select(XmlRpcStructConvertors.ToTestPlatform)
            ];
    }

    /// <summary>
    /// Gets all top level test suites for a test project.
    /// </summary>
    /// <param name="testProjectId">Id of the test project.</param>
    /// <returns>The test suites.</returns>
    public TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId)
    {
        var response = this.proxy.GetFirstLevelTestSuitesForTestProject(this.devkey, testProjectId);
        var errors = DecodeErrors(response);
        if (errors.Count > 0 && errors[0].Code != 7008) // 7008 means project has no test suites
        {
            CheckErrorMessage(response);
        }

        return
        [
            .. response
                .Cast<XmlRpcStruct>()
                .Select(XmlRpcStructConvertors.ToTestSuite)
        ];
    }

    /// <inheritdoc/>
    public TestSuite[] GetTestSuitesForTestSuite(int testSuiteId)
    {
        var response = this.proxy.GetTestSuitesForTestSuite(this.devkey, testSuiteId);
        if (response is string) // Testlink returns an empty string if a test suite has no child test suites
        {
            return [];
        }

        // just in case this gets fixed, then this should work.
        return CheckErrorMessage(response, 7008)
            ? []
            :
            [
                .. ((object[])response)
                .Cast<XmlRpcStruct>()
                .Select(XmlRpcStructConvertors.ToTestSuite)
            ];
    }

    /// <summary>
    /// Gets a test suite by its id.
    /// </summary>
    /// <param name="id">Id of the test suite.</param>
    /// <returns>The test suite if found, otherwise null.</returns>
    public TestSuite? GetTestSuiteById(int id)
    {
        var response = this.proxy.GetTestSuiteById(this.devkey, id);
        return CheckErrorMessage(response, 8000)
            ? null
            : XmlRpcStructConvertors.ToTestSuite((XmlRpcStruct)response);
    }

    /// <summary>
    /// Checks if the developer key exists.
    /// </summary>
    /// <param name="devKey">The dev key.</param>
    /// <returns><see langword="true"/> if key exists; otherwise <see langword="false"/>.</returns>
    public bool CheckDevKeyExists(string devKey)
    {
        var response = this.proxy.CheckDevKey(devKey);

        CheckErrorMessage(response);

        return (bool)response;
    }

    /// <summary>
    /// Checks for user id to see whether it exists.
    /// </summary>
    /// <param name="username">The user name.</param>
    /// <returns><see langword="true"/> if the user exists; otherwise <see langword="false"/>.</returns>
    public bool CheckUserExists(string username)
    {
        var response = this.proxy.DoesUserExist(this.devkey, username);
        return !CheckErrorMessage(response, 10000) && (bool)response;
    }

    /// <summary>
    /// Processes the response object returned by the Testlink API for error messages. 
    /// </summary>
    /// <param name="errorMessage">The actual message returned by testlink</param>
    /// <param name="exceptedErrorCodes">A list of expected error code contained in error messages</param>
    /// <returns>
    /// <see langword="true"/>: if it found an error message that matches an errorCodes list <br/>
    /// <see langword="false"/>: if there were no errors.</returns>
    /// <exception cref="TestLinkApiException">Thrown if any of the error messages are not in the exceptedErrorCodes list</exception>
    private static bool CheckErrorMessage(object errorMessage, params int[] exceptedErrorCodes)
    {
        if (errorMessage is not object[] errorMessages)
        {
            return false;
        }

        var errors = DecodeErrors(errorMessages);
        if (errors.Count == 0)
        {
            return false; // There were no errors
        }

        foreach (var error in errors)
        {
            if (exceptedErrorCodes.Any(errorCode => errorCode == error.Code))
            {
                continue;
            }

            throw new TestLinkApiException(
                $"Error with '{error.Code}' wasn't found in provided expected error code. Error message: {error.Message}");
        }

        return true; // The errors matched to the expectations
    }

    private static List<TestLinkErrorMessage> DecodeErrors(object[] messages)
        =>
        [
            .. messages
                .Cast<XmlRpcStruct>()
                .Where(message => message.ContainsKey("code") && message.ContainsKey("message"))
                .Select(XmlRpcStructConvertors.ToTestLinkErrorMessage)
        ];

    private void HandleResponseEvent(object sender, XmlRpcResponseEventArgs args)
    {
        args.ResponseStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(args.ResponseStream);
        this.LastDebugResponse = streamReader.ReadToEnd();
    }

    private void HandleRequestEvent(object sender, XmlRpcRequestEventArgs args)
    {
        args.RequestStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(args.RequestStream);
        this.LastDebugRequest = streamReader.ReadToEnd();
    }
}