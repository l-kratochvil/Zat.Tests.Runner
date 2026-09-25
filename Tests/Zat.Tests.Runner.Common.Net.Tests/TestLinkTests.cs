namespace Zat.Tests.Runner.Common.Net.Tests;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Net.TestLinkApi;

[TestFixture]
public class TestLinkTests
{
    private static readonly ITestLink.Config DefaultConfig = ITestLink.Config.Default;

    // TODO: Create v TL own project for testing purposes
    private const int TestPlanId = 9560;

    private const int CtorTestCaseOrderNumber = 0;

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkCtor_WhenCalledWithValidArgs_ThenNoExceptionsThrown()
    {
        Assert.DoesNotThrow(() => _ = new TestLink(DefaultConfig));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkCtor_WhenCalledWithUriEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        var currentConfig = DefaultConfig with
        {
            XmlRpcServerUrl = string.Empty,
        };

        Assert.Throws<TestLinkApiException>(() => _ = new TestLink(currentConfig));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkCtor_WhenCalledWithApiKeyEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        var currentConfig = DefaultConfig with
        {
            ApiKey = string.Empty,
        };

        Assert.Throws<TestLinkApiException>(() => _ = new TestLink(currentConfig));
    }

    [TestCase]
    public void GetBuildsForTestPlan_WhenValidTestPlanId_ThenShouldReturnSomeProjects()
    {
        // Given:
        var client = new TestLink(DefaultConfig);

        // When:
        var projects = client.GetBuildsForTestPlan(TestPlanId);

        // Then:
        Assert.That(projects, Has.Length.GreaterThan(0));
    }
}