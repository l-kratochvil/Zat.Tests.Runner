namespace Zat.Tests.Runner.Common.Net.TestLink.API;

using CookComputing.XmlRpc;

using Zat.Tests.Runner.Common.Net.TestLink.API.Model;

// API DOCS: https://www.jetmore.org/john/misc/phpdoc-testlink193-api/TestlinkAPI/TestlinkXMLRPCServer.html
// DEV-NOTE: DO NOT RENAME PARAMETERS: The mapping between method parameters and XML-RPC parameters relies on these exact names including their casing.

/// <summary>
/// the interface mapping required for the XmlRpc api of testlink.
/// This interface is used by the TestLink class.
/// </summary>
[XmlRpcUrl("")]
public interface ITestLinkXmlRpcProxy : IXmlRpcProxy
{
    [XmlRpcMethod("tl.createBuild", StructParams = true)]
    object[] CreateBuild(string devKey, int testplanid, string buildname, string buildnotes);

    [XmlRpcMethod("tl.getBuildsForTestPlan", StructParams = true)]
    object GetBuildsForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getProjects", StructParams = true)]
    object GetProjects(string devKey);

    [XmlRpcMethod("tl.getTestProjectByName", StructParams = true)]
    object GetTestProjectByName(string devKey, string testprojectname);

    [XmlRpcMethod("tl.createTestProject", StructParams = true)]
    object CreateTestProject(
        string devKey, string testprojectname, string testcaseprefix, string notes = "");

    [XmlRpcMethod("tl.uploadTestProjectAttachment", StructParams = true)]
    object UploadTestProjectAttachment(
        string devKey,
        int testprojectid,
        string filename,
        string fileType,
        string content,
        string title,
        string description);

    #region TestCase

    [XmlRpcMethod("tl.createTestCase", StructParams = true)]
    object CreateTestCase(
        string devKey,
        string authorlogin,
        int testsuiteid,
        string testcasename,
        int testprojectid,
        string summary,
        TestStep[] steps,
        string keywords,
        int order,
        int checkduplicatedname,
        string actiononduplicatedname,
        int executiontype,
        int importance);

    [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
    object AddTestCaseToTestPlan(
        string devKey,
        int testprojectid,
        int testplanid,
        string testcaseexternalid,
        int version);

    [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
    object AddTestCaseToTestPlan(
        string devKey,
        int testprojectid,
        int testplanid,
        string testcaseexternalid,
        int version,
        int platformid);

    [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
    object AddTestCaseToTestPlan(
        string devKey,
        int testprojectid,
        int testplanid,
        string testcaseexternalid,
        int version,
        int platformid,
        int executionorder,
        int urgency);

    [XmlRpcMethod("tl.getTestCaseAttachments", StructParams = true)]
    object GetTestCaseAttachments(string devKey, int testcaseid);

    [XmlRpcMethod("tl.getTestCaseCustomFieldDesignValue", StructParams = true)]
    object GetTestCaseCustomFieldDesignValue(
        string devKey,
        int testcaseid,
        string testcaseexternalid,
        int version,
        int testprojectid,
        string customfieldname,
        string details);

    /// <summary>
    /// Gets a test case by its external ID and version.
    /// <param name="devKey">The developer key for authentication (required).</param>
    /// <param name="testcaseid">The ID of the test case (optional).</param>
    /// <param name="testcaseexternalid">The external ID of the test case (required).</param>
    /// <param name="version">The version of the test case (required).</param>
    /// </summary>
    [XmlRpcMethod("tl.getTestCase", StructParams = true)]
    object GetTestCaseByExternalId(string devKey, string testcaseexternalid);

    /// <summary>
    /// Gets a test case by its ID.
    /// <param name="devKey">The developer key for authentication.</param>
    /// <param name="testcaseid">The ID of the test case.</param>
    /// </summary>
    [XmlRpcMethod("tl.getTestCase", StructParams = true)]
    object GetTestCaseById(string devKey, int testcaseid);

    [XmlRpcMethod("tl.getTestCaseIDByName", StructParams = true)]
    object GetTestCaseIdByName(string devKey, string testcasename, string testsuitename);

    [XmlRpcMethod("tl.getTestCaseIDByName", StructParams = true)]
    object GetTestCaseIdByName(string devKey, string testcasename);

    [XmlRpcMethod("tl.getTestCase", StructParams = true)]
    object GetTestCase(string devKey, int testcaseid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(
        string devKey, int testplanid, int testcaseid, int buildid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(
        string devKey, int testplanid, int testcaseid, int buildid, int keywordid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(
        string devKey,
        int testplanid,
        int testcaseid,
        int buildid,
        int keywordid,
        bool executed);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(
        string devKey,
        int testplanid,
        int testcaseid,
        int buildid,
        int keywordid,
        bool executed,
        int assignedTo);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(
        string devKey,
        int testplanid,
        int testcaseid,
        int buildid,
        int keywordid,
        bool executed,
        int assignedTo,
        string executedstatus);

    [XmlRpcMethod("tl.getTestCaseAssignedTester", StructParams = true)]
    object GetTestCaseAssignedTester(
        string devKey,
        int testplanid,
        int testcaseid,
        int platformid,
        int buildid);

    [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
    object GetTestCasesForTestSuite(string devKey, int testsuiteid);

    [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
    object GetTestCasesForTestSuite(string devKey, int testsuiteid, bool deep);

    [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
    object GetTestCasesForTestSuite(
        string devKey, int testsuiteid, bool deep, string details);

    [XmlRpcMethod("tl.uploadTestCaseAttachment", StructParams = true)]
    object UploadTestCaseAttachment(
        string devKey,
        int testcaseid,
        string filename,
        string filetype,
        string content,
        string title,
        string description);

    #endregion

    #region TestSuite

    [XmlRpcMethod("tl.getTestSuiteByID", StructParams = true)]
    object GetTestSuiteById(string devKey, int testsuiteid);

    [XmlRpcMethod("tl.getTestSuitesForTestPlan", StructParams = true)]
    object GetTestSuitesForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getFirstLevelTestSuitesForTestProject", StructParams = true)]
    object[] GetFirstLevelTestSuitesForTestProject(string devKey, int testprojectid);

    [XmlRpcMethod("tl.getTestSuitesForTestSuite", StructParams = true)]
    object GetTestSuitesForTestSuite(string devKey, int testsuiteid);

    [XmlRpcMethod("tl.createTestSuite", StructParams = true)]
    object[] CreateTestSuite(
        string devKey,
        int testprojectid,
        string testsuitename,
        string details,
        int parentid,
        int order,
        bool checkduplicatedname);

    [XmlRpcMethod("tl.createTestSuite", StructParams = true)]
    object[] CreateTestSuite(
        string devKey,
        int testprojectid,
        string testsuitename,
        string details,
        int order,
        bool checkduplicatedname);

    [XmlRpcMethod("tl.uploadTestSuiteAttachment", StructParams = true)]
    object UploadTestSuiteAttachment(
        string devKey,
        int testsuiteid,
        string filename,
        string fileType,
        string content,
        string title,
        string description);

    #endregion

    #region execution

    [XmlRpcMethod("tl.getLastExecutionResult", StructParams = true)]
    object[] GetLastExecutionResult(string devKey, int testplanid, int testcaseid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTcResult(
        string devKey,
        int testcaseid,
        int testplanid,
        string status,
        int platformid,
        bool overwrite,
        string notes,
        bool guess,
        int bugid,
        int buildid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTcResult(
        string devKey,
        int testcaseid,
        int testplanid,
        string status,
        string platformname,
        bool overwrite,
        string notes,
        bool guess,
        int bugid,
        int buildid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTcResult(
        string devKey,
        int testcaseid,
        int testplanid,
        string status,
        int platformid,
        bool overwrite,
        string notes,
        bool guess,
        int bugid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTcResult(
        string devKey,
        int testcaseid,
        int testplanid,
        string status,
        string platformname,
        bool overwrite,
        string notes,
        bool guess,
        int bugid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTcResult(
        string devKey,
        int testcaseid,
        int testplanid,
        string status,
        int platformid,
        bool overwrite,
        string notes,
        bool guess);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTcResult(
        string devKey,
        int testcaseid,
        int testplanid,
        string status,
        string platformname,
        bool overwrite,
        string notes,
        bool guess);

    [XmlRpcMethod("tl.deleteExecution", StructParams = true)]
    object DeleteExecution(string devKey, int executionid);

    [XmlRpcMethod("tl.uploadExecutionAttachment", StructParams = true)]
    object UploadExecutionAttachment(
        string devKey,
        int executionid,
        string filename,
        string fileType,
        string content,
        string title,
        string description);

    #endregion

    #region Testplan

    [XmlRpcMethod("tl.getTestPlanPlatforms", StructParams = true)]
    object GetTestPlanPlatforms(string devKey, int testplanid);

    [XmlRpcMethod("tl.createTestPlan", StructParams = true)]
    object[] CreateTestPlan(
        string devKey,
        string testplanname,
        string testprojectname,
        string notes,
        string active); // can't do parameter called 'public' as it collides with .net

    [XmlRpcMethod("tl.getProjectTestPlans", StructParams = true)]
    object[] GetProjectTestPlans(string devKey, int testprojectid);

    [XmlRpcMethod("tl.getLatestBuildForTestPlan", StructParams = true)]
    object GetLatestBuildForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getTestPlanByName", StructParams = true)]
    object[] GetTestPlanByName(
        string devKey, string testprojectname, string testplanname);

    [XmlRpcMethod("tl.getTotalsForTestPlan", StructParams = true)]
    object GetTotalsForTestPlan(string devKey, int testplanid);

    #endregion

    #region other

    [XmlRpcMethod("tl.sayHello")]
    string SayHello();

    [XmlRpcMethod("tl.doesUserExist", StructParams = true)]
    object DoesUserExist(string devKey, string user);

    [XmlRpcMethod("tl.checkDevKey", StructParams = true)]
    object CheckDevKey(string devKey);

    [XmlRpcMethod("tl.about")]
    string About();

    [XmlRpcMethod("tl.getFullPath", StructParams = true)]
    object GetFullPath(string devKey, int nodeId);

    #endregion
}