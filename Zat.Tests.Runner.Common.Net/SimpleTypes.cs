namespace Zat.Tests.Runner.Common.Net;

using Zat.Z2xxTests.Common;

public record TestResult(
    ProxyTestResult ProxyTestResult,
    TestedHwAssemblyType? TestedHwAssemblyType = null)
    : ProxyTestResult(ProxyTestResult);