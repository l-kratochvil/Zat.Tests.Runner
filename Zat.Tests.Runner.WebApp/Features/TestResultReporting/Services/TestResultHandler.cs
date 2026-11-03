namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Services;

using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestResultReporting;

public class TestResultHandler(GlobalWeakReferenceMessanger globalMessanger) : ITestResultHandler
{
    /// <inheritdoc />
    public void HandleSubRunResult(TestResult testResult)
        => globalMessanger.Send(new SubRunResultReceivedAction(testResult));
}