namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The directories every application has; an application derives from it to add its own.
/// </summary>
/// <param name="AppData">Full path of the directory the application keeps its data in.</param>
/// <param name="Logs">Full path of the directory the log files are written to.</param>
public record SharedAppDirectoryPaths(
    string AppData,
    string Logs);