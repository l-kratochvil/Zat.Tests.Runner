namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Components;

using System.Globalization;

using Fluxor;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.WebApp.Features.TestResultReporting.Models;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestResultReporting;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

using TestRunResult = Zat.Tests.Runner.Common.Net.Model.TestResult;

public sealed class TestResultViewModel : ViewModelBase, IDisposable
{
    private readonly IState<TestResultReportingState> state;

    public TestResultViewModel(IState<TestResultReportingState> state)
    {
        this.state = state;
        this.state.StateChanged += this.OnStateChanged;
    }

    /// <summary>
    /// Gets the completed sub-runs as reports for the result panel.
    /// </summary>
    public IReadOnlyList<SubRunReport> SubRunReports
        => [.. this.state.Value.SubRunResults.Select((result, index) => CreateReport(result, index + 1))];

    /// <inheritdoc/>
    public void Dispose()
        => this.state.StateChanged -= this.OnStateChanged;

    private static SubRunReport CreateReport(TestRunResult result, int order)
    {
        var titleParts = new List<string>
        {
            $"Sub-run {order}",
            result.TestType.ToString(),
        };
        if (result.TestedHwAssemblyType is { } testedHwAssemblyType)
        {
            titleParts.Add(testedHwAssemblyType.ToString());
        }

        titleParts.Add(result.StartTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss'Z'", CultureInfo.InvariantCulture));

        return new SubRunReport(
            string.Join(" · ", titleParts),
            MapStatus(result.OverallStatus),
            [.. result.ReportedNotRunResults.Select(
                entity => CreateEntry(entity, MapNotRunLabel(entity.Status), "label-skipped", includeStackTrace: false))],
            [.. result.ReportedProblemResults.Select(
                entity => CreateEntry(entity, MapProblemLabel(entity.Status), "label-problem", includeStackTrace: true))],
            result);
    }

    private static ReportEntry CreateEntry(
        TestEntityResult entity,
        string label,
        string labelClass,
        bool includeStackTrace)
    {
        var detail = entity.Detail;
        var message = detail?.Message;
        var stackTrace = detail?.StackTrace;
        if (includeStackTrace && !string.IsNullOrEmpty(stackTrace))
        {
            message = string.IsNullOrEmpty(message)
                ? stackTrace
                : $"{message}{Environment.NewLine}{stackTrace}";
        }

        return new ReportEntry(label, labelClass, entity.EntityName, message);
    }

    private static string MapNotRunLabel(TestStatus status)
        => status switch
        {
            TestStatus.Ignored => "Ignored",
            TestStatus.Explicit => "Explicit",
            TestStatus.Skipped => "Skipped",
            _ => "Unknown",
        };

    private static string MapProblemLabel(TestStatus status)
        => status switch
        {
            TestStatus.Error => "Error",
            TestStatus.Invalid => "Invalid",
            TestStatus.Failure => "Failed",
            _ => "Warning",
        };

    private static string MapStatus(TestStatus status)
        => status switch
        {
            TestStatus.Passed => "Passed",
            TestStatus.Failure or TestStatus.Error or TestStatus.Invalid => "Failed",
            TestStatus.Skipped or TestStatus.Ignored or TestStatus.Explicit => "Skipped",
            TestStatus.Inconclusive => "Inconclusive",
            TestStatus.Warning => "Warning",
            _ => "Unknown",
        };

    private void OnStateChanged(object? sender, EventArgs e)
        => this.OnPropertyChanged(nameof(this.SubRunReports));
}