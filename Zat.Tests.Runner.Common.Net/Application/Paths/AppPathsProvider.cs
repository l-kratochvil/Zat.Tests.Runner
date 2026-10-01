namespace Zat.Tests.Runner.Common.Net.Application.Paths;

using DevKit.Core.Interfaces;
using Microsoft.Extensions.Options;
using Zat.Z2xxTests.Common;

/// <summary>
/// The application paths, derived from the configured application data path.
/// </summary>
public class AppPathsProvider : IAppPathsProvider, IInitializable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppPathsProvider"/> class.
    /// </summary>
    /// <param name="sharedAppOptions">The shared application options.</param>
    public AppPathsProvider(IOptions<ISharedAppOptions> sharedAppOptions)
        : this(sharedAppOptions.Value.LocalAppDataPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppPathsProvider"/> class.
    /// </summary>
    /// <param name="localAppDataPath">The local application data path.</param>
    internal AppPathsProvider(string localAppDataPath)
    {
        this.Extensions = new AppFileExtensions(
            Log: ".log");

        this.DirectoryNames = new AppDirectoryNames();

        this.FileNames = new AppFileNames(
            MainAssemblyDll: "Zat.Z2xxTests.dll");

        this.Directories = new AppDirectoryPaths(
            AppData: localAppDataPath,
            Logs: Path.Combine(localAppDataPath, "logs"));

        this.Files = new AppFilePaths(
            UserSettings: Path.Combine(localAppDataPath, "user-settings.json"),
            MainAssemblyDll: Path.Combine(
                Paths.Directories.TestLibs, this.FileNames.MainAssemblyDll));
    }

    /// <inheritdoc/>
    public AppDirectoryPaths Directories { get; }

    /// <inheritdoc/>
    public AppFilePaths Files { get; }

    /// <inheritdoc/>
    public AppFileExtensions Extensions { get; }

    /// <inheritdoc/>
    public AppFileNames FileNames { get; }

    /// <inheritdoc/>
    public AppDirectoryNames DirectoryNames { get; }

    /// <summary>
    /// Creates the directories the application writes into.
    /// </summary>
    public void Initialize()
    {
        Directory.CreateDirectory(this.Directories.AppData);
        Directory.CreateDirectory(this.Directories.Logs);
    }
}