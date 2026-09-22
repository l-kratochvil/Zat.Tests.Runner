namespace Zat.Tests.Runner.Common;

using Zat.Tests.Runner.Common.Model;

public record ProxyTestResult(
    TestStatus Status,
    TestRunSummary Summary,
    IgnoredResult[] IgnoredResults,
    ExplicitResult[] ExplicitResults,
    OtherSkippedResult[] OtherResults,
    ErrorResult[] ErrorResults,
    InvalidResult[] InvalidResults,
    FailureResult[] FailureResults,
    WarningResult[] WarningResults)
{
    public TestStatus Status { get; } = Status;

    public TestRunSummary Summary { get; } = Summary;

    public IgnoredResult[] IgnoredResults { get; } = IgnoredResults;

    public ExplicitResult[] ExplicitResults { get; } = ExplicitResults;

    public OtherSkippedResult[] OtherResults { get; } = OtherResults;

    public ErrorResult[] ErrorResults { get; } = ErrorResults;

    public InvalidResult[] InvalidResults { get; } = InvalidResults;

    public FailureResult[] FailureResults { get; } = FailureResults;

    public WarningResult[] WarningResults { get; } = WarningResults;
}

public record TestRunSummary(
    int Total,
    int Passed,
    int Failed,
    int Warnings,
    int Inconclusive,
    int Skipped,
    int Failures,
    int Errors,
    int Invalid,
    int Ignored,
    int Explicit,
    int Other)
{
    public int Total { get; } = Total;

    public int Passed { get; } = Passed;

    public int Failed { get; } = Failed;

    public int Warnings { get; } = Warnings;

    public int Inconclusive { get; } = Inconclusive;

    public int Skipped { get; } = Skipped;

    public int Failures { get; } = Failures;

    public int Errors { get; } = Errors;

    public int Invalid { get; } = Invalid;

    public int Ignored { get; } = Ignored;

    public int Explicit { get; } = Explicit;

    public int Other { get; } = Other;
}

public record IgnoredResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record ExplicitResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record OtherSkippedResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record FailureResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record ErrorResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record InvalidResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record WarningResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record UnsuccessfulResult(
    string EntityName,
    string Message,
    string StackTrace)
{
    public string EntityName { get; } = EntityName;

    public string Message { get; } = Message;

    public string StackTrace { get; } = StackTrace;
}