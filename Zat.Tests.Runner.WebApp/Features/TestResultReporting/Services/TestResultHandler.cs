namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Services;

using Zat.Tests.Runner.Common;
using Zat.Tests.Runner.Common.Net.Services;

public class TestResultHandler : ITestResultHandler
{
    /// <inheritdoc />
    public void Handle(TestRunResult testRunResult)
    {
        System.Diagnostics.Debug.Fail("TODO");
    }
}