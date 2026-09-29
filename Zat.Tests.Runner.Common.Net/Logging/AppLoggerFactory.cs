namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Creates application loggers writing into the shared hub.
/// </summary>
/// <param name="loggerHub">Hub the created loggers append to.</param>
public sealed class AppLoggerFactory(IAppLoggerHub loggerHub) : IAppLoggerFactory
{
    /// <inheritdoc/>
    public IAppLogger CreateLogger(string source)
        => new AppLogger(loggerHub, source);
}