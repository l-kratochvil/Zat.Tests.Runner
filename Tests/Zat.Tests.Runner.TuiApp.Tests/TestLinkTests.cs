namespace Zat.Tests.Runner.TuiApp.Tests;

// TOOD: Remove this reference (only NUnitRunnerProxy should know about NUnit)
using NUnit.Framework;

using static Zat.Tests.Runner.TuiApp.Common.InternalTypes;

// TODO: Vytvo�it v TL vlastn� projekt pro ��ely testov�n� TestLinkApi

[TestFixture]
public class TestLinkTests
{
    private static readonly AppSystemConfig DefaultAppSystemConfig = new(
        TestLinkConfig: new TestLinkConfig(
            ApiKey: "dc7a17e14a9f1879d38583a38c3a81e8",
            XmlRpcServerUrl: "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php",
            LoggingEnabled: false));

    private const int TestPlanId = 9560;

    private const int CtorTestCaseOrderNumber = 0;

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithValidArgs_ThenNoExceptionsThrown()
    {
        Assert.DoesNotThrow(() => _ = new TestLinkApiClient(DefaultAppSystemConfig));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithUriEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        var currentConfig = DefaultAppSystemConfig with
        {
            TestLinkConfig = DefaultAppSystemConfig.TestLinkConfig with
            {
                XmlRpcServerUrl = string.Empty,
            },
        };
        Assert.Throws<Zat.Tests.Runner.Common.Net.TestLink.API.TestLinkApiException>(() => _ = new TestLinkApiClient(currentConfig));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithApiKeyEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        var currentConfig = DefaultAppSystemConfig with
        {
            TestLinkConfig = DefaultAppSystemConfig.TestLinkConfig with
            {
                ApiKey = string.Empty,
            },
        };

        Assert.Throws<Zat.Tests.Runner.Common.Net.TestLink.API.TestLinkApiException>(() => _ = new TestLinkApiClient(currentConfig));
    }

    [TestCase]
    public void GetBuildsForTestPlan_WhenValidTestPlanId_ThenShouldReturnSomeProjects()
    {
        // Arrange
        var client = new TestLinkApiClient(DefaultAppSystemConfig);

        // Act
        var projects = client.GetBuildsForTestPlan(TestPlanId);

        // Assert
        Assert.That(projects, Has.Length.GreaterThan(0));
    }
}