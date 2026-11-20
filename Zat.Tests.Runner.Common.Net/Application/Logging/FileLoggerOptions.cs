namespace Zat.Tests.Runner.Common.Net.Application.Logging;

/// <summary>
/// Options that configure the file logger, bound from <c>Logging:File</c>.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>
    /// Gets how many log files are kept on disk.
    /// </summary>
    public int RetainedFileCount { get; init; } = 5;
}