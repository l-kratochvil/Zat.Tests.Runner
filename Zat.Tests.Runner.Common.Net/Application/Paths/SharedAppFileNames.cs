namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The file names every application uses; an application derives from it to add its own.
/// </summary>
/// <param name="MainAssemblyDll">File name of the main assembly DLL.</param>
public record SharedAppFileNames(
    string MainAssemblyDll);