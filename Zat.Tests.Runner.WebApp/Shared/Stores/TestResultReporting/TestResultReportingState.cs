namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestResultReporting;

using Fluxor;

using Zat.Tests.Runner.Common.Net.Model;

[FeatureState]
public sealed record TestResultReportingState(IReadOnlyList<TestResult> SubRunResults)
{
    public TestResultReportingState()
        : this([])
    {
    }
}