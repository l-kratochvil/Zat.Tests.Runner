namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Watches a directory through a <see cref="FileSystemWatcher"/>, which can't watch a directory that isn't there and
/// stops for good on an error. Either way it is started again, every <see cref="RetryInterval"/> until it can be.
/// </summary>
/// <param name="timeProvider">Measures the time between attempts to start watching again.</param>
public sealed class FileSystemDirectoryWatcher(TimeProvider timeProvider) : IDirectoryWatcher
{
    /// <summary>
    /// How long to wait before trying again to watch a directory that couldn't be watched.
    /// </summary>
    internal static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    /// <remarks>
    /// A directory that is missing is watched once it appears, and its appearing counts as a change.
    /// </remarks>
    public IDisposable Watch(string directoryPath, Action onChanged)
        => new Watching(directoryPath, onChanged, timeProvider);

    private sealed class Watching : IDisposable
    {
        private readonly string directoryPath;
        private readonly Action onChanged;
        private readonly ITimer retryTimer;

        // Guards the watcher, which the watcher's own threads, the retry timer and Dispose touch at once.
        private readonly Lock watcherLock = new();

        private FileSystemWatcher? watcher;
        private bool disposed;

        public Watching(string directoryPath, Action onChanged, TimeProvider timeProvider)
        {
            this.directoryPath = directoryPath;
            this.onChanged = onChanged;
            this.retryTimer = timeProvider.CreateTimer(
                _ => this.Start(announce: true),
                state: null,
                dueTime: Timeout.InfiniteTimeSpan,
                period: Timeout.InfiniteTimeSpan);

            this.Start(announce: false);
        }

        public void Dispose()
        {
            lock (this.watcherLock)
            {
                this.disposed = true;
                this.retryTimer.Dispose();
                this.watcher?.Dispose();
                this.watcher = null;
            }
        }

        /// <param name="announce">Whether watching starts again after a while nobody watched the directory, in
        /// which anything may have changed.</param>
        private void Start(bool announce)
        {
            lock (this.watcherLock)
            {
                if (this.disposed || this.watcher is not null)
                {
                    return;
                }

                try
                {
                    this.watcher = this.CreateWatcher();
                }
                catch (Exception exception) when (exception is ArgumentException or IOException)
                {
                    this.retryTimer.Change(dueTime: RetryInterval, period: Timeout.InfiniteTimeSpan);
                    return;
                }
            }

            if (announce)
            {
                this.onChanged();
            }
        }

        private FileSystemWatcher CreateWatcher()
        {
            var fileSystemWatcher = new FileSystemWatcher(this.directoryPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.LastWrite
                               | NotifyFilters.Size,
            };

            fileSystemWatcher.Created += (_, _) => this.onChanged();
            fileSystemWatcher.Changed += (_, _) => this.onChanged();
            fileSystemWatcher.Deleted += (_, _) => this.onChanged();
            fileSystemWatcher.Renamed += (_, _) => this.onChanged();
            fileSystemWatcher.Error += (_, _) => this.OnError();

            fileSystemWatcher.EnableRaisingEvents = true;

            return fileSystemWatcher;
        }

        private void OnError()
        {
            // The watcher lost track of what happened, so anything may have changed, and it may watch no more.
            lock (this.watcherLock)
            {
                this.watcher?.Dispose();
                this.watcher = null;

                if (!this.disposed)
                {
                    this.retryTimer.Change(dueTime: RetryInterval, period: Timeout.InfiniteTimeSpan);
                }
            }

            this.onChanged();
        }
    }
}