namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The files of this application.
/// </summary>
/// <param name="UserSettings">Full path of the file the application settings are kept in.</param>
/// <param name="MainAssemblyDll">Full path of the main assembly DLL.</param>
public sealed record AppFilePaths(
    string UserSettings,
    string MainAssemblyDll);