namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The file extensions every application uses; an application derives from it to add its own.
/// </summary>
/// <param name="Log">Extension of the log files.</param>
public record SharedAppFileExtensions(
    string Log);