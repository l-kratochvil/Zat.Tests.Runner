namespace Zat.Tests.Runner.TuiApp.Screens;

using System.Diagnostics;
using System.Globalization;

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
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
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

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = async ct =>
            {
                var state = new State();

                var startTime = DateTime.UtcNow;
                state.Stopwatch.Start();

                var table = new Table()
                    .HideHeaders()
                    .AddColumn(string.Empty)
                    .AddColumn(string.Empty);

                var runTestTask = testRunnerEngine.RunTestAsync(
                    testConfigStore.SelectedTestEntities, new TestConfig(
                        testedRuntimeVersion: testConfigStore.RuntimeVersion,
                        testedHwAssemblyType: testConfigStore.TestedHwAssemblyType,
                        isDebug: testConfigStore.IsDebug));

                var promptResult = await ShowLiveDataAsync(
                    table,
                    state,
                    async (table, data, ctx, ct) =>
                    {
                        while (!ct.IsCancellationRequested && !runTestTask.IsCompleted)
                        {
                            // TODO: Show test logs?
                            table.Rows.Clear();
                            table.AddRow(Resources.ElapsedTime, $"{data.Stopwatch.Elapsed:hh\\:mm\\:ss}");
                            ctx.Refresh();

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

                var testResult = await runTestTask;

                state.Stopwatch.Stop();
                var endTime = DateTime.UtcNow;

                RenderNotRunSection(testResult);
                RenderProblemsSection(testResult);
                RenderSummarySection(
                    testResult,
                    startTime,
                    endTime,
                    state.Stopwatch.Elapsed);

                // TODO: Prompt whether to send result to TestLink (it will redirect to the TestLinkInfoPromptScreen)
                // TODO: Save the test result to XML file (that can be imported to TestLink) just in case
                WriteLine(string.Empty);
                MarkupLine($"[aqua]{Resources.PressAnyKeyToContinue_Message.EscapeMarkup()}[/]");

                await AnsiConsole.Console.Input.ReadKeyAsync(true, ct);

                return CompletedShowPrompt.Default;
            },
        };

    private static void RenderNotRunSection(TestRunResult result)
    {
        var entries = new List<ReportEntry>();
        entries.AddRange(result.IgnoredResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Ignored, x)));
        entries.AddRange(result.ExplicitResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Explicit, x)));
        entries.AddRange(result.OtherResults.Select(
            x => new ReportEntry("yellow", Resources.TestRunReport_Label_Skipped, x)));

        RenderEntrySection(Resources.TestRunReport_TestsNotRun_SectionHeader, entries, includeStackTrace: false);
    }

    private static void RenderProblemsSection(TestRunResult result)
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

        RenderEntrySection(Resources.TestRunReport_ErrorsFailuresWarnings_SectionHeader, entries, includeStackTrace: true);
    }

    private static void RenderEntrySection(string header, IReadOnlyList<ReportEntry> entries, bool includeStackTrace)
    {
        if (entries.Count == 0)
        {
            return;
        }

        WriteLine();
        MarkupLine($"[aqua]{header.EscapeMarkup()}[/]");
        WriteLine();

        for (var i = 0; i < entries.Count; i++)
        {
            if (i > 0)
            {
                WriteLine();
            }

            var entry = entries[i];
            var headerLine = $"{i + 1}) {entry.Label} : {entry.Result.EntityName}";

            if (entry.Color is null)
            {
                WriteLine(headerLine);
            }
            else
            {
                MarkupLine($"[{entry.Color}]{headerLine.EscapeMarkup()}[/]");
            }

            if (!string.IsNullOrEmpty(entry.Result.Message))
            {
                WriteLine(entry.Result.Message);
            }

            if (includeStackTrace && !string.IsNullOrEmpty(entry.Result.StackTrace))
            {
                WriteLine(entry.Result.StackTrace);
            }
        }
    }

    private static void RenderSummarySection(
        TestRunResult result,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        TimeSpan duration)
    {
        var summary = result.Summary;

        WriteLine();
        MarkupLine($"[aqua]{Resources.TestRunReport_Summary_SectionHeader.EscapeMarkup()}[/]");

        MarkupLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_OverallResult)} {MapStatus(result.Status).EscapeMarkup()}");

        MarkupLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_TestCount)} {summary.Total}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Passed)} {summary.Passed}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Failed)} {summary.Failed}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Warnings)} {summary.Warnings}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Inconclusive)} {summary.Inconclusive}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Skipped)} {summary.Skipped}");

        MarkupLine(
            $"    [green]{Resources.TestRunReport_Summary_FailedTests.EscapeMarkup()} -[/]" +
            $" {MakeLabel(Resources.TestRunReport_Summary_Failures)} {summary.Failures}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Errors)} {summary.Errors}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Invalid)} {summary.Invalid}");

        MarkupLine(
            $"    [green]{Resources.TestRunReport_Summary_SkippedTests.EscapeMarkup()} -[/]" +
            $" {MakeLabel(Resources.TestRunReport_Summary_Ignored)} {summary.Ignored}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Explicit)} {summary.Explicit}" +
            $", {MakeLabel(Resources.TestRunReport_Summary_Other)} {summary.Other}");

        MarkupLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_StartTime)} {FormatTimestamp(startTimeUtc)}");
        MarkupLine(
            $"  {MakeLabel(Resources.TestRunReport_Summary_EndTime)} {FormatTimestamp(endTimeUtc)}");
        MarkupLine(
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
}