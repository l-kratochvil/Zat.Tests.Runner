namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Tells when anything inside a directory changes on disk.
/// </summary>
public interface IDirectoryWatcher
{
    /// <summary>
    /// Starts watching <paramref name="directoryPath"/>, its subdirectories included.
    /// </summary>
    /// <remarks>
    /// Watching outlasts the directory: one missing or replaced is watched again once it is there, so a test
    /// assembly deployed later is still noticed.
    /// </remarks>
    /// <param name="directoryPath">The directory to watch.</param>
    /// <param name="onChanged">Called on every change, on a thread of the watcher; a single change on disk may
    /// call it several times.</param>
    /// <returns>Stops watching when disposed.</returns>
    IDisposable Watch(string directoryPath, Action onChanged);
}