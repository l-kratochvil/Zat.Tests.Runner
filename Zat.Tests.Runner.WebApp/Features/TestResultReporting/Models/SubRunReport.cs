namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Models;

using Zat.Tests.Runner.Common.Net.Model;

public sealed record SubRunReport(
    string Title,
    string OverallStatus,
    IReadOnlyList<ReportEntry> NotRunEntries,
    IReadOnlyList<ReportEntry> ProblemEntries,
    TestResult Result);