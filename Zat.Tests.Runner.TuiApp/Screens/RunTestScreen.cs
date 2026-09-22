namespace Zat.Tests.Runner.TuiApp.Screens;

using System.Diagnostics;
using System.Globalization;
using System.Text;

using DevKit.Core.Utils;

using WindowsInput.Native;

using Zat.Tests.Runner.Common;
using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp.Common;
using Zat.Tests.Runner.TuiApp.Stores;
using Zat.Z2xxTests.Common.Model;

internal class RunTestScreen(
    TestConfigStore testConfigStore,
    ITestRunnerEngine testRunnerEngine,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen),
      ITestResultHandler
{
    private readonly List<TestResultHandled> handledTestResults = [];
    private readonly Stopwatch currentTestStopwatch = new();
    private DateTime currentTestStartTime = DateTime.UtcNow;

    /// <inheritdoc/>
    protected override Configuration Config { get; init; } = new()
    {
        IsHomeCommandEnabled = false,
        IsBackCommandEnabled = false,
        IsSettingsCommandEnabled = false,
    };

    /// <inheritdoc/>
    protected override ICommand[] AdditionalCommands
        => field ??=
        [
            ..base.AdditionalCommands,
            new ActionCommand(
                Key: VirtualKeyCode.F2,
                Text: Resources.StopTest_CommandText,
                Action: testRunnerEngine.StopTestRun)
        ];

    /// <inheritdoc />
    void ITestResultHandler.Handle(TestRunResult result)
    {
        var oldTestStartTime = this.currentTestStartTime;
        this.currentTestStartTime = DateTime.UtcNow;
        this.handledTestResults.Add(
            new TestResultHandled(
                result,
                oldTestStartTime,
                DateTime.UtcNow,
                this.currentTestStopwatch.Elapsed));
        this.currentTestStopwatch.Restart();
    }

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = async ct =>
            {
                this.handledTestResults.Clear();

                var state = new State();
                var table = new Table()
                    .HideHeaders()
                    .AddColumn(string.Empty);

                this.currentTestStopwatch.Start();
                this.currentTestStartTime = DateTime.UtcNow;

                var runTestTask = testRunnerEngine.RunTestAsync(
                    testRunEntities: testConfigStore.SelectedTestEntities,
                    testedRuntimeVersion: testConfigStore.RuntimeVersion,
                    testedHwAssemblyTypes: testConfigStore.HwAssemblyTypes,
                    isDebug: testConfigStore.IsDebug,
                    testResultHandlers: [this]);

                var promptResult = await ShowLiveDataAsync(
                    table,
                    state,
                    async (table, data, ctx, ct) =>
                    {
                        while (true)
                        {
                            var sb = new StringBuilder();
                            for (var i = 0; i < this.handledTestResults.Count; i++)
                            {
                                var handledTestResult = this.handledTestResults[i];

                                sb.AppendLine(string.Empty);
                                sb.AppendLine(TextUtils.SafeFormat(
                                    Resources.TestResultHeader_Format,
                                    i + 1,
                                    handledTestResult.StartTime));

                                var testResult = handledTestResult.Result;
                                RenderNotRunSection(sb, testResult);
                                RenderProblemsSection(sb, testResult);
                                RenderSummarySection(
                                    sb,
                                    testResult,
                                    handledTestResult.StartTime,
                                    handledTestResult.EndTime,
                                    handledTestResult.Duration);
                            }

                            table.Rows.Clear();
                            table.AddRow(new Markup($"{Resources.ElapsedTime}: {data.Stopwatch.Elapsed:hh\\:mm\\:ss}"));
                            table.AddRow(new Markup(sb.ToString()));

                            ctx.Refresh();

                            if (runTestTask.IsCompleted)
                            {
                                break;
                            }

                            await Task.Delay(100, ct);
                        }
                    },
                    _ => RenderOutput.Default,
                    ct);

                if (promptResult is InterruptedShowPrompt interuptedShowPrompt)
                {
                    testRunnerEngine.StopTestRun();
                    return interuptedShowPrompt;
                }

                await runTestTask;

                this.currentTestStopwatch.Stop();
                state.Stopwatch.Stop();

                MarkupLine($"[aqua]{Resources.PressAnyKeyToContinue_Message.EscapeMarkup()}[/]");

                await AnsiConsole.Console.Input.ReadKeyAsync(true, ct);

                return CompletedShowPrompt.Default;
            },
        };

    private static void RenderNotRunSection(StringBuilder sb, TestRunResult result)
    {
        var entries = new List<ReportEntry>();
        entries.AddRange(result.IgnoredResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Ignored, x)));
        entries.AddRange(result.ExplicitResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Explicit, x)));
        entries.AddRange(result.OtherResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Skipped, x)));

        RenderEntrySection(sb, Resources.TestRunReport_TestsNotRun_SectionHeader, entries, includeStackTrace: false);
    }

    private static void RenderProblemsSection(StringBuilder sb, TestRunResult result)
    {
        var entries = new List<ReportEntry>();
        entries.AddRange(result.ErrorResults.Select(
            x => new ReportEntry("red", Resources.TestRunReport_Label_Error, x)));
        entries.AddRange(result.InvalidResults.Select(
            x => new ReportEntry("red", Resources.TestRunReport_Label_Invalid, x)));
        entries.AddRange(result.FailureResults.Select(
            x => new ReportEntry("red", Resources.TestRunReport_Label_Failed, x)));
        entries.AddRange(result.WarningResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Warning, x)));

        RenderEntrySection(sb, Resources.TestRunReport_ErrorsFailuresWarnings_SectionHeader, entries, includeStackTrace: true);
    }

    private static void RenderEntrySection(
        StringBuilder sb, string header, IReadOnlyList<ReportEntry> entries, bool includeStackTrace)
    {
        if (entries.Count == 0)
        {
            return;
        }

        sb.AppendLine();
        sb.AppendLine($"[aqua]{header.EscapeMarkup()}[/]");
        sb.AppendLine();

        for (var i = 0; i < entries.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            var entry = entries[i];
            var headerLine = $"{i + 1}) {entry.Label} : {entry.Result.EntityName}";

            sb.AppendLine(entry.Color is null
                ? headerLine.EscapeMarkup()
                : $"[{entry.Color}]{headerLine.EscapeMarkup()}[/]");

            if (!string.IsNullOrEmpty(entry.Result.Message))
            {
                sb.AppendLine(entry.Result.Message.EscapeMarkup());
            }

            if (includeStackTrace && !string.IsNullOrEmpty(entry.Result.StackTrace))
            {
                sb.AppendLine(entry.Result.StackTrace.EscapeMarkup());
            }
        }
    }

    private static void RenderSummarySection(
        StringBuilder sb,
        TestRunResult result,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        TimeSpan duration)
    {
        var summary = result.Summary;

        sb.AppendLine();
        sb.AppendLine($"[aqua]{Resources.TestRunReport_Summary_SectionHeader.EscapeMarkup()}[/]");

        sb.AppendLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_OverallResult)} {MapStatus(result.Status).EscapeMarkup()}");

        sb.AppendLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_TestCount)} {summary.Total}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Passed)} {summary.Passed}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Failed)} {summary.Failed}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Warnings)} {summary.Warnings}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Inconclusive)} {summary.Inconclusive}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Skipped)} {summary.Skipped}");

        sb.AppendLine(
            $"    [green]{Resources.TestRunReport_Summary_FailedTests.EscapeMarkup()} -[/]" +
            $" {MakeLabel(Resources.TestRunReport_Summary_Failures)} {summary.Failures}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Errors)} {summary.Errors}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Invalid)} {summary.Invalid}");

        sb.AppendLine(
            $"    [green]{Resources.TestRunReport_Summary_SkippedTests.EscapeMarkup()} -[/]" +
            $" {MakeLabel(Resources.TestRunReport_Summary_Ignored)} {summary.Ignored}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Explicit)} {summary.Explicit}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Other)} {summary.Other}");

        sb.AppendLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_StartTime)} {FormatTimestamp(startTimeUtc)}");
        sb.AppendLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_EndTime)} {FormatTimestamp(endTimeUtc)}");
        sb.AppendLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_Duration)} " +
            $"{duration.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)} " +
            $"{Resources.TestRunReport_Summary_Seconds.EscapeMarkup()}");
    }

    private static string MakeLabel(string text)
        => $"[green]{text.EscapeMarkup()}:[/]";

    private static string FormatTimestamp(DateTime utc)
        => utc.ToString("yyyy-MM-dd HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    private static string MapStatus(TestStatus status)
        => status switch
        {
            TestStatus.Passed => Resources.TestRunReport_Status_Passed,
            TestStatus.Failed => Resources.TestRunReport_Status_Failed,
            TestStatus.Skipped => Resources.TestRunReport_Status_Skipped,
            TestStatus.Inconclusive => Resources.TestRunReport_Status_Inconclusive,
            TestStatus.Warning => Resources.TestRunReport_Status_Warning,
            _ => Resources.TestRunReport_Status_Unknown,
        };

    private record ReportEntry(
        string? Color,
        string Label,
        UnsuccessfulResult Result);

    private class State
    {
        public Stopwatch Stopwatch { get; } = new();
    }

    private record TestResultHandled(
        TestRunResult Result,
        DateTime StartTime,
        DateTime EndTime,
        TimeSpan Duration);
}