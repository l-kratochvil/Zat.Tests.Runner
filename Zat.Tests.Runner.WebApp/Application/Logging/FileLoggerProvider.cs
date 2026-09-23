namespace Zat.Tests.Runner.WebApp.Application.Logging;

using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Zat.Tests.Runner.WebApp.Application.Paths;
using ILogger = ILogger;

/// <summary>
/// An <see cref="ILoggerProvider"/> that writes diagnostics to the log file.
/// </summary>
/// <remarks>
/// The file itself is written by Serilog, which the provider keeps behind its own alias so that
/// filtering stays with the logging pipeline through <c>Logging:File:LogLevel</c>.
/// </remarks>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<string, ILogger> loggers = new(StringComparer.Ordinal);
    private readonly Logger fileLogger;
    private readonly SerilogLoggerProvider provider;

    private readonly Lock gate = new();

    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
    /// </summary>
    /// <param name="options">Options that configure the file logger.</param>
    /// <param name="paths">Provider of the application paths.</param>
    public FileLoggerProvider(IOptions<FileLoggerOptions> options, IAppPathsProvider paths)
    {
        const string outputTempalte = "{Timestamp:yyyy-MM-dd HH:mm:ss:fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        this.fileLogger = new LoggerConfiguration()
            .WriteTo
            .File(
                path: Path.Combine(paths.Directories.Logs, ".log"),
                outputTemplate: outputTempalte,
                rollingInterval: options.Value.RetainedFileCount > 0 ? RollingInterval.Day : RollingInterval.Infinite,
                retainedFileCountLimit: options.Value.RetainedFileCount)
            .CreateLogger();

        this.provider = new SerilogLoggerProvider(this.fileLogger);
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
        => this.loggers.GetOrAdd(categoryName, this.provider.CreateLogger);

    /// <inheritdoc/>
    public void Dispose()
    {
        // The provider is disposed both by the logger factory and by the container, so this runs
        // more than once.
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.provider.Dispose();

        // Draining happens during shutdown, so it must not be able to hang the process on a
        // stalled disk: whatever is left after the timeout is dropped.
        Task.Run(this.fileLogger.Dispose).Wait(FlushTimeout);
    }
}