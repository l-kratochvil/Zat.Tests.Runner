using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

const string testLinkApiKey = "dc7a17e14a9f1879d38583a38c3a81e8";

const string testLinkUrl = "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php";

var apiClient = new TestLinkApiClient(ITestLinkApiClient.Config.Default);

const int productionProjectId = 6302; // TOTO JE ID PRODUKČNÍHO TEST PROJECTU
const int tempProjectId = 10202;
const int testProjectId = productionProjectId;

var projectTestsuites = apiClient.GetFirstLevelTestSuitesForTestProject(testProjectId);
var testsuite = apiClient.GetTestSuiteById(9572);
var info = GetInformationForTester(testsuite);
var testSuites = GetAllTestSuitesAndTestCases(testProjectId);

Console.WriteLine("DONE");

List<TestSuite> GetAllTestSuitesAndTestCases(int testProjectId)
{
    var testSuitesForTestProject = apiClient.GetFirstLevelTestSuitesForTestProject(testProjectId);
    var suites = new List<TestSuite>();

    foreach (var testSuite in testSuitesForTestProject)
    {
        var _ts = GetTestSuitesAndCases(testSuite);

        suites.Add(_ts);
    }

    return suites;
}

string GetInformationForTester(TestSuite testSuite)
{
    var text = TransformFromHTMLDocToText(testSuite);
    return GetMatchedTextForTester(text);
}

string TransformFromHTMLDocToText(TestSuite testSuite)
{
    var doc = new HtmlDocument();
    doc.LoadHtml(testSuite._details);

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
        testSuite._id,
        testSuite._name,
        testSuite._details,
        testSuite._nodeOrder,
        testSuite._nodeTypeId,
        testSuite._parentId);
    var tc = apiClient.GetTestCasesForTestSuite(testSuite._id, false);
    var ts = apiClient.GetTestSuitesForTestSuite(testSuite._id);

    for (var i = 0; i < tc.Length; i++)
    {
        suite.AddTestCase(tc[i]);
    }

    if (ts.Length > 0)
    {
        for (var i = 0; i < ts.Length; i++)
        {
            var childSuite = GetTestSuitesAndCases(ts[i]);
            suite.AddTestSuite(childSuite);
        }
    }

    return suite;
}

void SaveTestResults(
    string build,
    (TestStatus status, int testPlanId, int testSuiteId, int testCaseId, string notes)[] Result)
{
    foreach (var result in Result)
    {
        var testPlatform = apiClient.GetTestPlanPlatforms(result.testPlanId).First();

        if (!apiClient.GetBuildsForTestPlan(result.testPlanId).Any(x => x.name == build))
        {
            apiClient.CreateBuild(result.testPlanId, build, string.Empty);
        }

        var testBuild = apiClient.GetBuildsForTestPlan(result.testPlanId).First(x => x.name == build);

        // NOTE: testcase/testsuite ID se získá: Specifikace testů >> pravé tl. myši na test. příp. ve stromu
        var testsuiteTestcases =
            apiClient.GetTestCasesForTestSuite(result.testSuiteId, true);

        var
            testcase = testsuiteTestcases.First(x
                => x.external_id == result.testCaseId.ToString()); // 44 je číselná složka z ID ve formátu Z200-XX (Z200-44)
        var testcaseApiId = testcase.id;

        var resultStatus = result.status switch
        {
            TestStatus.Passed => "p",
            TestStatus.Failed => "f",
            TestStatus.Skipped => "b",
            _ => string.Empty,
        };

        var res = apiClient.UploadTestCaseExecutionResult(
            testcaseApiId,
            result.testPlanId,
            resultStatus,
            platformId: testPlatform.id, // Platforma musí být přidána do testovacího plánu. Pokud není potřeba specifikovat platformu, tak stačí zadat prázdný string do argument platfromName
            overwrite: false,
            notes: result.notes,
            buildId: testBuild.id);
    }
}