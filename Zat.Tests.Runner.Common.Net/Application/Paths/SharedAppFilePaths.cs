namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The files every application has; an application derives from it to add its own.
/// </summary>
/// <param name="UserSettings">Full path of the file the application settings are kept in.</param>
/// <param name="MainAssemblyDll">Full path of the main assembly DLL.</param>
public record SharedAppFilePaths(
    string UserSettings,
    string MainAssemblyDll);