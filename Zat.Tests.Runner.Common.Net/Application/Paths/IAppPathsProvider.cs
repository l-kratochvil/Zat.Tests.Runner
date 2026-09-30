namespace Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// Where this application keeps everything it reads and writes.
/// </summary>
/// <remarks>
/// The single answer to the question, so that no path is composed anywhere else. Reading a path
/// touches no disk; the directories are created when the service is initialised.
/// </remarks>
public interface IAppPathsProvider
{
    /// <summary>
    /// Gets the directories of the application.
    /// </summary>
    AppDirectoryPaths Directories { get; }

    /// <summary>
    /// Gets the files of the application.
    /// </summary>
    AppFilePaths Files { get; }

    /// <summary>
    /// Gets the file extensions used by the application.
    /// </summary>
    AppFileExtensions Extensions { get; }

    /// <summary>
    /// Gets the file names used by the application.
    /// </summary>
    AppFileNames FileNames { get; }

    /// <summary>
    /// Gets the directory names used by the application.
    /// </summary>
    AppDirectoryNames DirectoryNames { get; }
}