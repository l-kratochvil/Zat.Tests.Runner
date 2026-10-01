namespace Zat.Tests.Runner.Common.Net.Application.Paths;

using DevKit.Core.Interfaces;
using Microsoft.Extensions.Options;
using Zat.Z2xxTests.Common;

/// <summary>
/// The application paths, derived from the configured application data path.
/// </summary>
/// <remarks>
/// The shared paths are composed here alone; an application adds its own by deriving each group
/// from the shared one in the matching <c>Create*</c> method. Each group is composed on first read,
/// so the methods may read the other groups through <see langword="this"/>.
/// </remarks>
/// <typeparam name="TDirectories">The directories of the application.</typeparam>
/// <typeparam name="TFiles">The files of the application.</typeparam>
/// <typeparam name="TExtensions">The file extensions used by the application.</typeparam>
/// <typeparam name="TFileNames">The file names used by the application.</typeparam>
/// <typeparam name="TDirectoryNames">The directory names used by the application.</typeparam>
public abstract class AppPathsProviderBase<TDirectories, TFiles, TExtensions, TFileNames, TDirectoryNames>
    : ISharedAppPathsProvider, IInitializable
    where TDirectories : SharedAppDirectoryPaths
    where TFiles : SharedAppFilePaths
    where TExtensions : SharedAppFileExtensions
    where TFileNames : SharedAppFileNames
    where TDirectoryNames : SharedAppDirectoryNames
{
    private readonly SharedAppDirectoryPaths sharedDirectories;
    private readonly SharedAppFilePaths sharedFiles;
    private readonly SharedAppFileExtensions sharedExtensions;
    private readonly SharedAppFileNames sharedFileNames;
    private readonly SharedAppDirectoryNames sharedDirectoryNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppPathsProviderBase{TDirectories, TFiles, TExtensions, TFileNames, TDirectoryNames}"/> class.
    /// </summary>
    /// <param name="sharedAppOptions">The shared application options.</param>
    protected AppPathsProviderBase(IOptions<ISharedAppOptions> sharedAppOptions)
        : this(sharedAppOptions.Value.LocalAppDataPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppPathsProviderBase{TDirectories, TFiles, TExtensions, TFileNames, TDirectoryNames}"/> class.
    /// </summary>
    /// <param name="localAppDataPath">The local application data path.</param>
    protected AppPathsProviderBase(string localAppDataPath)
    {
        this.sharedExtensions = new SharedAppFileExtensions(
            Log: ".log");

        this.sharedDirectoryNames = new SharedAppDirectoryNames();

        this.sharedFileNames = new SharedAppFileNames(
            MainAssemblyDll: "Zat.Z2xxTests.dll");

        this.sharedDirectories = new SharedAppDirectoryPaths(
            AppData: localAppDataPath,
            Logs: Path.Combine(localAppDataPath, "logs"));

        this.sharedFiles = new SharedAppFilePaths(
            UserSettings: Path.Combine(localAppDataPath, "user-settings.json"),
            MainAssemblyDll: Path.Combine(
                Paths.Directories.TestLibs, this.sharedFileNames.MainAssemblyDll));
    }

    /// <inheritdoc cref="ISharedAppPathsProvider.Directories"/>
    public TDirectories Directories
        => field ??= this.CreateDirectories(this.sharedDirectories);

    /// <inheritdoc cref="ISharedAppPathsProvider.Files"/>
    public TFiles Files
        => field ??= this.CreateFiles(this.sharedFiles);

    /// <inheritdoc cref="ISharedAppPathsProvider.Extensions"/>
    public TExtensions Extensions
        => field ??= this.CreateExtensions(this.sharedExtensions);

    /// <inheritdoc cref="ISharedAppPathsProvider.FileNames"/>
    public TFileNames FileNames
        => field ??= this.CreateFileNames(this.sharedFileNames);

    /// <inheritdoc cref="ISharedAppPathsProvider.DirectoryNames"/>
    public TDirectoryNames DirectoryNames
        => field ??= this.CreateDirectoryNames(this.sharedDirectoryNames);

    /// <inheritdoc/>
    SharedAppDirectoryPaths ISharedAppPathsProvider.Directories
        => this.Directories;

    /// <inheritdoc/>
    SharedAppFilePaths ISharedAppPathsProvider.Files
        => this.Files;

    /// <inheritdoc/>
    SharedAppFileExtensions ISharedAppPathsProvider.Extensions
        => this.Extensions;

    /// <inheritdoc/>
    SharedAppFileNames ISharedAppPathsProvider.FileNames
        => this.FileNames;

    /// <inheritdoc/>
    SharedAppDirectoryNames ISharedAppPathsProvider.DirectoryNames
        => this.DirectoryNames;

    /// <summary>
    /// Creates the directories the application writes into.
    /// </summary>
    public void Initialize()
    {
        Directory.CreateDirectory(this.Directories.AppData);
        Directory.CreateDirectory(this.Directories.Logs);
    }

    /// <summary>
    /// Creates the directories of the application from the <paramref name="shared"/> ones.
    /// </summary>
    /// <param name="shared">The directories every application has.</param>
    /// <returns>The directories of the application.</returns>
    protected abstract TDirectories CreateDirectories(SharedAppDirectoryPaths shared);

    /// <summary>
    /// Creates the files of the application from the <paramref name="shared"/> ones.
    /// </summary>
    /// <param name="shared">The files every application has.</param>
    /// <returns>The files of the application.</returns>
    protected abstract TFiles CreateFiles(SharedAppFilePaths shared);

    /// <summary>
    /// Creates the file extensions of the application from the <paramref name="shared"/> ones.
    /// </summary>
    /// <param name="shared">The file extensions every application uses.</param>
    /// <returns>The file extensions of the application.</returns>
    protected abstract TExtensions CreateExtensions(SharedAppFileExtensions shared);

    /// <summary>
    /// Creates the file names of the application from the <paramref name="shared"/> ones.
    /// </summary>
    /// <param name="shared">The file names every application uses.</param>
    /// <returns>The file names of the application.</returns>
    protected abstract TFileNames CreateFileNames(SharedAppFileNames shared);

    /// <summary>
    /// Creates the directory names of the application from the <paramref name="shared"/> ones.
    /// </summary>
    /// <param name="shared">The directory names every application uses.</param>
    /// <returns>The directory names of the application.</returns>
    protected abstract TDirectoryNames CreateDirectoryNames(SharedAppDirectoryNames shared);
}