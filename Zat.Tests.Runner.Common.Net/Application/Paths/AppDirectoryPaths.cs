namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The directories of this application.
/// </summary>
/// <param name="AppData">Full path of the directory the application keeps its data in.</param>
/// <param name="Logs">Full path of the directory the log files are written to.</param>
public sealed record AppDirectoryPaths(
    string AppData,
    string Logs);