namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting;

using Fluxor;
using JetBrains.Annotations;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestExecution;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestResultReporting;

public static class Reducers
{
    [ReducerMethod]
    [UsedImplicitly]
    public static TestResultReportingState OnSubRunResultReceived(
        TestResultReportingState current,
        SubRunResultReceivedAction action)
        => current with { SubRunResults = [.. current.SubRunResults, action.TestResult] };

    [ReducerMethod]
    [UsedImplicitly]
    public static TestResultReportingState OnTestRunStarted(
        TestResultReportingState current,
        TestRunStartedAction action)
        => current with { SubRunResults = [] };
}