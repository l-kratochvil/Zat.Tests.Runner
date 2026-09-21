namespace Zat.Tests.Runner.Common.Net.Services;

using DevKit.Core.UtilityObjects;

using Zat.Z2xxTests.Common.Model;

public interface ITestRunnerBridgeConnector
{
    public Disposer Connect(TestConfig testConfig);
}