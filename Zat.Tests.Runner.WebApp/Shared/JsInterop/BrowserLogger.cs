namespace Zat.Tests.Runner.WebApp.Shared.JsInterop;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Routes browser diagnostics into the diagnostics pipeline.
/// </summary>
/// <remarks>
/// Browser diagnostics reach the log file only; they never become tester-facing log entries.
/// </remarks>
/// <param name="loggerFactory">Factory used to create the browser diagnostics logger.</param>
public sealed class BrowserLogger(ILoggerFactory loggerFactory)
{
    /// <summary>
    /// Logger category used for browser diagnostics.
    /// </summary>
    /// <remarks>
    /// The whole browser shares one category; the script name stays in each diagnostic.
    /// </remarks>
    public const string Category = "Zat.Tests.Runner.WebApp.Browser";

    private readonly ILogger logger = loggerFactory.CreateLogger(Category);

    /// <summary>
    /// Writes one browser diagnostic.
    /// </summary>
    /// <param name="diagnostic">Browser diagnostic to write.</param>
    [JSInvokable]
    public void Log(BrowserDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        var level = GetLevel(diagnostic.Level);

        if (string.IsNullOrEmpty(diagnostic.Detail))
        {
            this.logger.Log(level, "[{Module}] {Message}", diagnostic.Module, diagnostic.Message);

            return;
        }

        // The detail stays a value of its own instead of being concatenated into the message, so
        // that the message template keeps its meaning for structured destinations. Destinations
        // rendering plain text see it as the lines following the message.
        this.logger.Log(
            level,
            "[{Module}] {Message}\n{Detail}",
            diagnostic.Module,
            diagnostic.Message,
            diagnostic.Detail);
    }

    /// <summary>
    /// Maps browser severity onto <see cref="LogLevel"/>.
    /// </summary>
    /// <remarks>
    /// An unknown severity is kept and raised to <see cref="LogLevel.Warning"/> so the bad value
    /// stays visible.
    /// </remarks>
    /// <param name="level">Severity as the browser spells it.</param>
    /// <returns><see cref="LogLevel"/> used for the diagnostic.</returns>
    private static LogLevel GetLevel(string? level)
        => level?.ToLowerInvariant() switch
        {
            "debug" => LogLevel.Debug,
            "info" => LogLevel.Information,
            "warn" => LogLevel.Warning,
            "error" => LogLevel.Error,
            _ => LogLevel.Warning,
        };
}