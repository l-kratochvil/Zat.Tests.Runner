namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Services;

using System.Diagnostics;

using DevKit.Core.Extensions.Types;

using Zat.Tests.Runner.Common.Net;
using Zat.Tests.Runner.Common.Net.Services;

public class TestResultHandler : ITestResultHandler
{
    /// <inheritdoc />
    public void Handle(TestResult testResult)
    {
        Debug.SafeFail("TODO");
    }
}