namespace Zat.Tests.Runner.Common.Net;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common.Model;

public record TestResult(
    ProxyTestResult ProxyTestResult,
    TestType TestType,
    HwAssemblyType? TestedHwAssemblyType = null)
    : ProxyTestResult(ProxyTestResult);