namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Watches a directory through a <see cref="FileSystemWatcher"/>.
/// </summary>
public sealed class FileSystemDirectoryWatcher : IDirectoryWatcher
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> does not exist.</exception>
    public IDisposable Watch(string directoryPath, Action onChanged)
    {
        var watcher = new FileSystemWatcher(directoryPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Size,
        };

        watcher.Created += (_, _) => onChanged();
        watcher.Changed += (_, _) => onChanged();
        watcher.Deleted += (_, _) => onChanged();
        watcher.Renamed += (_, _) => onChanged();

        // The watcher lost track of what happened, so anything may have changed.
        watcher.Error += (_, _) => onChanged();

        watcher.EnableRaisingEvents = true;

        return watcher;
    }
}
