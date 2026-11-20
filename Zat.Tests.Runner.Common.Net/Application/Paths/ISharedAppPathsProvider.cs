namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// Where every application keeps what it shares with the others.
/// </summary>
/// <remarks>
/// The single answer to the question, so that no path is composed anywhere else. Shared code asks
/// for this; the paths only one application uses are reachable through that application's own
/// provider. Reading a path touches no disk; the directories are created when the service is
/// initialised.
/// </remarks>
public interface ISharedAppPathsProvider
{
    /// <summary>
    /// Gets the directories of the application.
    /// </summary>
    SharedAppDirectoryPaths Directories { get; }

    /// <summary>
    /// Gets the files of the application.
    /// </summary>
    SharedAppFilePaths Files { get; }

    /// <summary>
    /// Gets the file extensions used by the application.
    /// </summary>
    SharedAppFileExtensions Extensions { get; }

    /// <summary>
    /// Gets the file names used by the application.
    /// </summary>
    SharedAppFileNames FileNames { get; }

    /// <summary>
    /// Gets the directory names used by the application.
    /// </summary>
    SharedAppDirectoryNames DirectoryNames { get; }
}