namespace Zat.Tests.Runner.WebApp.Features.TestResultReporting.Models;

public sealed record ReportEntry(
    string Label,
    string LabelClass,
    string Name,
    string? Detail);