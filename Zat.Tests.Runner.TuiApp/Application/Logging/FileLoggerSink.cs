namespace Zat.Tests.Runner.TuiApp.Application.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Zat.Tests.Runner.Common.Net.Logging;

public class FileLoggerSink : IAppLoggerSink
{
    private readonly Logger logger;

    public FileLoggerSink()
    {
        const string outputTempalte = "{Timestamp:yyyy-MM-dd HH:mm:ss:fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        this.logger = new LoggerConfiguration()
            .WriteTo
            .File(
                path: Path.Combine(Paths.Directories.Logs, Paths.FileNames.Log),
                outputTemplate: outputTempalte,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 5)
            .CreateLogger();
    }

    /// <inheritdoc />
    public event Action<string>? Failed;

    /// <inheritdoc />
    public void Write(LogEntry entry)
        => this.logger.Write(
            entry.Severity switch
            {
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Error => LogEventLevel.Error,
                _ => LogEventLevel.Information,
            },
            entry.Message);
}