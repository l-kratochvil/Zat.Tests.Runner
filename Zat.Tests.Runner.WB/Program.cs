using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

const string testLinkApiKey = "dc7a17e14a9f1879d38583a38c3a81e8";

const string testLinkUrl = "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php";

var apiClient = new TestLink(ITestLink.Config.Default);

const int productionProjectId = 6302; // TOTO JE ID PRODUKČNÍHO TEST PROJECTU
const int tempProjectId = 10202;
const int testProjectId = productionProjectId;

var about = apiClient.About();
var testCase = apiClient.GetTestCaseByExternalId("Z200-240");
// var projectTestPlanPlatforms = apiClient.GetTestPlanPlatforms(10208);
// var projectTestsuites = apiClient.GetFirstLevelTestSuitesForTestProject(testProjectId);
// var testsuite = apiClient.GetTestSuiteById(9572);
// var info = GetInformationForTester(testsuite);
// var testSuites = GetAllTestSuitesAndTestCases(testProjectId);

Console.WriteLine("DONE");

List<TestSuite> GetAllTestSuitesAndTestCases(int testProjectId)
{
    var testSuitesForTestProject = apiClient.GetFirstLevelTestSuitesForTestProject(testProjectId);
    var suites = new List<TestSuite>();

    foreach (var testSuite in testSuitesForTestProject)
    {
        var ts = GetTestSuitesAndCases(testSuite);

        suites.Add(ts);
    }

    return suites;
}

string GetInformationForTester(TestSuite testSuite)
{
    var text = TransformFromHtmlDocToText(testSuite);
    return GetMatchedTextForTester(text);
}

string TransformFromHtmlDocToText(TestSuite testSuite)
{
    var doc = new HtmlDocument();
    doc.LoadHtml(testSuite.Details);

    return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
}

string GetMatchedTextForTester(string text)
{
    var pattern = @"\d+\.\d+\.\d+\s+Podmínky splněné testerem(\s*[\s\S]*?)\d+\.\d+\.\d+\s+Podmínky splněné vývojářem";
    var regex = new Regex(pattern);
    var match = regex.Match(text);

    var userText = match.Groups[1].Value.Trim();

    if (userText == string.Empty)
    {
        throw new Exception("The match for this text cannot be found.");
    }

    return userText;
}

TestSuite GetTestSuitesAndCases(TestSuite testSuite)
{
    var suite = new TestSuite(
        testSuite.Id,
        testSuite.Name,
        testSuite.Details,
        testSuite.NodeOrder,
        testSuite.NodeTypeId,
        testSuite.ParentId);
    var tc = apiClient.GetTestCasesForTestSuite(testSuite.Id, false);
    var ts = apiClient.GetTestSuitesForTestSuite(testSuite.Id);

    foreach (var t in tc)
    {
        suite.AddTestCase(t);
    }

    // ReSharper disable once InvertIf
    if (ts.Length <= 0)
    {
        foreach (var t in ts)
        {
            var childSuite = GetTestSuitesAndCases(t);
            suite.AddTestSuite(childSuite);
        }
    }

    return suite;
}