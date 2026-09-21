#pragma warning disable SA1124 // Do not use regions
namespace Zat.Tests.Runner.Common.Net.TestLinkApi;

using CookComputing.XmlRpc;
using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// the interface mapping required for the XmlRpc api of testlink.
/// This interface is used by the TestLink class.
/// </summary>
[XmlRpcUrl("")]
public interface ITestLinkXmlRpcProxy : IXmlRpcProxy
{
    //[XmlRpcMethod("tl.assignRequirements")]
    //string assignRequirements();

    [XmlRpcMethod("tl.createBuild", StructParams = true)]
    object[] CreateBuild(string devKey, int testplanid, string buildname, string buildnotes);

    [XmlRpcMethod("tl.getBuildsForTestPlan", StructParams = true)]
    object GetBuildsForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getProjects", StructParams = true)]
    object GetProjects(string devKey);

    [XmlRpcMethod("tl.getTestProjectByName", StructParams = true)]
    object GetTestProjectByName(string devKey, string testprojectname);

    [XmlRpcMethod("tl.createTestProject", StructParams = true)]
    object CreateTestProject(string devKey, string testprojectname, string testcaseprefix, string notes = "");

    [XmlRpcMethod("tl.uploadTestProjectAttachment", StructParams = true)]
    object UploadTestProjectAttachment(string devKey, int testprojectid, string filename, string fileType, string content, string title,
        string description);

    #region TestCase

    [XmlRpcMethod("tl.createTestCase", StructParams = true)]
    object CreateTestCase(string devKey, string authorlogin, int testsuiteid, string testcasename, int testprojectid,
        string summary, TestStep[] steps, string keywords,
        int order, int checkduplicatedname, string actiononduplicatedname, int executiontype, int importance);

    [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
    object AddTestCaseToTestPlan(string devKey, int testprojectid, int testplanid, string testcaseexternalid, int version);

    [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
    object AddTestCaseToTestPlan(string devKey, int testprojectid, int testplanid, string testcaseexternalid, int version, int platformid);

    [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
    object AddTestCaseToTestPlan(string devKey, int testprojectid, int testplanid, string testcaseexternalid, int version, int platformid,
        int executionorder, int urgency);

    [XmlRpcMethod("tl.getTestCaseAttachments", StructParams = true)]
    object GetTestCaseAttachments(string devKey, int testcaseid);

    [XmlRpcMethod("tl.getTestCaseCustomFieldDesignValue", StructParams = true)]
    object GetTestCaseCustomFieldDesignValue(string devKey, int testcaseid, string testcaseexternalid, int version, int testprojectid,
        string customfieldname, string details);

    [XmlRpcMethod("tl.getTestCaseIDByName", StructParams = true)]
    object GetTestCaseIDByName(string devKey, string testcasename, string testsuitename);

    [XmlRpcMethod("tl.getTestCaseIDByName", StructParams = true)]
    object GetTestCaseIDByName(string devKey, string testcasename);

    [XmlRpcMethod("tl.getTestCaseByExternalId", StructParams = true)]
    object GetTestCaseByExternalId(string devKey, int testcaseexternalid, int testprojectid);

    /// <summary>
    /// get test case specification using external or internal id. returns last version
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="testcaseid"></param>
    /// <returns></returns>
    [XmlRpcMethod("tl.getTestCase", StructParams = true)]
    object GetTestCase(string devKey, int testcaseid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="testcaseid"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    [XmlRpcMethod("tl.getTestCase", StructParams = true)]
    object GetTestCase(string devKey, int testcaseid, int version);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid, bool executed);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid, bool executed, int assignedTo);

    [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
    object GetTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid, bool executed, int assignedTo,
        string executedstatus);

    [XmlRpcMethod("tl.getTestCaseAssignedTester", StructParams = true)]
    object GetTestCaseAssignedTester(string devKey, int testplanid, int testcaseid, int platformid, int buildid);

    [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
    object GetTestCasesForTestSuite(string devKey, int testsuiteid);

    [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
    object GetTestCasesForTestSuite(string devKey, int testsuiteid, bool deep);

    [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
    object GetTestCasesForTestSuite(string devKey, int testsuiteid, bool deep, string details);

    [XmlRpcMethod("tl.uploadTestCaseAttachment", StructParams = true)]
    object UploadTestCaseAttachment(string devKey, int testcaseid, string filename, string filetype, string content, string title,
        string description);

    #endregion

    #region TestSuite

    [XmlRpcMethod("tl.getTestSuiteByID", StructParams = true)]
    object GetTestSuiteByID(string devKey, int testsuiteid);

    [XmlRpcMethod("tl.getTestSuitesForTestPlan", StructParams = true)]
    object GetTestSuitesForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getFirstLevelTestSuitesForTestProject", StructParams = true)]
    object[] GetFirstLevelTestSuitesForTestProject(string devKey, int testprojectid);

    [XmlRpcMethod("tl.getTestSuitesForTestSuite", StructParams = true)]
    object GetTestSuitesForTestSuite(string devKey, int testsuiteid);

    [XmlRpcMethod("tl.createTestSuite", StructParams = true)]
    object[] CreateTestSuite(string devKey, int testprojectid, string testsuitename, string details, int parentid, int order,
        bool checkduplicatedname);

    [XmlRpcMethod("tl.createTestSuite", StructParams = true)]
    object[] CreateTestSuite(string devKey, int testprojectid, string testsuitename, string details, int order, bool checkduplicatedname);

    [XmlRpcMethod("tl.uploadTestSuiteAttachment", StructParams = true)]
    object UploadTestSuiteAttachment(string devKey, int testsuiteid, string filename, string fileType, string content, string title,
        string description);

    #endregion

    #region execution

    [XmlRpcMethod("tl.getLastExecutionResult", StructParams = true)]
    object[] GetLastExecutionResult(string devKey, int testplanid, int testcaseid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTCResult(string devKey, int testcaseid, int testplanid, string status, int platformid, bool overwrite, string notes, bool guess,
        int bugid, int buildid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTCResult(string devKey, int testcaseid, int testplanid, string status, string platformname, bool overwrite, string notes,
        bool guess, int bugid, int buildid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTCResult(string devKey, int testcaseid, int testplanid, string status, int platformid, bool overwrite, string notes, bool guess,
        int bugid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTCResult(string devKey, int testcaseid, int testplanid, string status, string platformname, bool overwrite, string notes,
        bool guess, int bugid);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTCResult(string devKey, int testcaseid, int testplanid, string status, int platformid, bool overwrite, string notes, bool guess);

    [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
    object ReportTCResult(string devKey, int testcaseid, int testplanid, string status, string platformname, bool overwrite, string notes,
        bool guess);

    /// <summary>
    /// delete an execution
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="executionid"></param>
    /// <returns> mixed $resultInfo 
    /// 				[status]	=> true/false of success
    /// 				[id]		  => result id or error code
    /// 				[message]	=> optional message for error message string</returns>
    [XmlRpcMethod("tl.deleteExecution", StructParams = true)]
    object DeleteExecution(string devKey, int executionid);

    [XmlRpcMethod("tl.uploadExecutionAttachment", StructParams = true)]
    object UploadExecutionAttachment(string devKey, int executionid, string filename, string fileType, string content, string title,
        string description);

    #endregion

    #region Testplan

    /// <summary>
    /// 
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="testplanid"></param>
    /// <returns></returns>
    [XmlRpcMethod("tl.getTestPlanPlatforms", StructParams = true)]
    object GetTestPlanPlatforms(string devKey, int testplanid);

    [XmlRpcMethod("tl.createTestPlan", StructParams = true)]
    object[] CreateTestPlan(string devKey, string testplanname, string testprojectname, string notes,
        string active); // can't do parameter called 'public' as it collides with .net

    [XmlRpcMethod("tl.getProjectTestPlans", StructParams = true)]
    object[] GetProjectTestPlans(string devKey, int testprojectid);

    [XmlRpcMethod("tl.getLatestBuildForTestPlan", StructParams = true)]
    object GetLatestBuildForTestPlan(string devKey, int testplanid);

    [XmlRpcMethod("tl.getTestPlanByName", StructParams = true)]
    object[] GetTestPlanByName(string devKey, string testprojectname, string testplanname);

    /// <summary>
    /// Gets the summarized results grouped by platform
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="testplanid"></param>
    /// <returns>map where every element has:
    /// 	 *
    /// 	 *	'type' => 'platform'
    /// 	 *	'total_tc => ZZ
    /// 	 *	'details' => array ( 'passed' => array( 'qty' => X)
    /// 	 *	                     'failed' => array( 'qty' => Y)
    /// 	 *	                     'blocked' => array( 'qty' => U)
    /// 	 *                       ....)</returns>
    [XmlRpcMethod("tl.getTotalsForTestPlan", StructParams = true)]
    object GetTotalsForTestPlan(string devKey, int testplanid);

    #endregion

    #region other

    /// <summary>
    /// simple Ping.
    /// </summary>
    /// <returns></returns>
    [XmlRpcMethod("tl.sayHello")]
    string SayHello();

    /// <summary>
    /// checks user exists
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="user"></param>
    /// <returns>true if everything OK, otherwise error structure</returns>
    [XmlRpcMethod("tl.doesUserExist", StructParams = true)]
    object DoesUserExist(string devKey, string user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="devKey"></param>
    /// <returns>true if everything OK, otherwise error structure</returns>
    [XmlRpcMethod("tl.checkDevKey", StructParams = true)]
    object CheckDevKey(string devKey);

    [XmlRpcMethod("tl.about")]
    string About();

    /// <summary>
    /// Gets full path from the given node till the top using nodes_hierarchy_table
    /// </summary>
    /// <param name="devKey"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    [XmlRpcMethod("tl.getFullPath", StructParams = true)]
    object GetFullPath(string devKey, int nodeID);

    //[XmlRpcMethod("tl.repeat")]
    //string repeat();

    #endregion
}