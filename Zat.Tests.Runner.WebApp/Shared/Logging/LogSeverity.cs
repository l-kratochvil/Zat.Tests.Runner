namespace Zat.Tests.Runner.WebApp.Shared.Logging;

/// <summary>
/// Severity of a log entry.
/// </summary>
/// <remarks>
/// The log has no debug severity; diagnostics use <see cref="ILogger"/>
/// instead.
/// </remarks>
public enum LogSeverity
{
    /// <summary>
    /// Normal progress, including successful outcomes.
    /// </summary>
    Info,

    /// <summary>
    /// Something unexpected happened, but the application keeps working.
    /// </summary>
    Warning,

    /// <summary>
    /// A failed operation.
    /// </summary>
    Error,
}