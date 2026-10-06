namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using Microsoft.Extensions.Time.Testing;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// The watcher runs against a real directory, as what it adapts is the file system itself; only the time between
/// attempts to start watching again is faked.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class FileSystemDirectoryWatcherTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly string directoryPath = Path.Combine(Path.GetTempPath(), $"{nameof(FileSystemDirectoryWatcherTests)}-{Guid.NewGuid():N}");
    private readonly FakeTimeProvider timeProvider = new();
    private readonly FileSystemDirectoryWatcher unit;

    public FileSystemDirectoryWatcherTests()
        => this.unit = new FileSystemDirectoryWatcher(this.timeProvider);

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.directoryPath))
        {
            Directory.Delete(this.directoryPath, recursive: true);
        }
    }

    [Test]
    public async Task Watch__WhenFileInSubdirectoryIsWritten__ThenShouldTellAboutIt()
    {
        // Given:
        Directory.CreateDirectory(Path.Combine(this.directoryPath, "Sub"));
        var changed = new TaskCompletionSource();
        using var watching = this.unit.Watch(this.directoryPath, () => changed.TrySetResult());

        // When:
        await File.WriteAllTextAsync(Path.Combine(this.directoryPath, "Sub", "Test.dll"), "changed");

        // Then:
        Assert.That(async () => await changed.Task.WaitAsync(Timeout), Throws.Nothing);
    }

    [Test]
    public async Task Watch__WhenStopped__ThenShouldNoLongerTellAboutChanges()
    {
        // Given:
        Directory.CreateDirectory(this.directoryPath);
        var changeCount = 0;
        var watching = this.unit.Watch(this.directoryPath, () => Interlocked.Increment(ref changeCount));

        // When:
        watching.Dispose();
        await File.WriteAllTextAsync(Path.Combine(this.directoryPath, "Test.dll"), "changed");
        this.timeProvider.Advance(FileSystemDirectoryWatcher.RetryInterval);
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        // Then:
        Assert.That(changeCount, Is.Zero);
    }

    [Test]
    public async Task Watch__WhenDirectoryAppearsOnlyLater__ThenShouldTellAboutItAndWatchIt()
    {
        // Given:
        // A test machine the test assembly was not deployed to yet.
        var changeCount = 0;
        using var watching = this.unit.Watch(this.directoryPath, () => Interlocked.Increment(ref changeCount));
        Directory.CreateDirectory(this.directoryPath);

        // When:
        this.timeProvider.Advance(FileSystemDirectoryWatcher.RetryInterval);
        var changesWhenDirectoryAppeared = Volatile.Read(ref changeCount);
        await File.WriteAllTextAsync(Path.Combine(this.directoryPath, "Test.dll"), "changed");

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(changesWhenDirectoryAppeared, Is.EqualTo(1));
            Assert.That(
                async () => await WaitUntilAsync(() => Volatile.Read(ref changeCount) > changesWhenDirectoryAppeared),
                Throws.Nothing);
        }
    }

    [Test]
    public async Task Watch__WhenWatchedDirectoryIsDeletedAndMadeAgain__ThenShouldGoOnTellingAboutChanges()
    {
        // Given:
        // A deployment that replaces the whole directory ends the watcher of the old one.
        Directory.CreateDirectory(this.directoryPath);
        var changeCount = 0;
        using var watching = this.unit.Watch(this.directoryPath, () => Interlocked.Increment(ref changeCount));
        Directory.Delete(this.directoryPath);
        await WaitUntilAsync(() => Volatile.Read(ref changeCount) > 0);
        await WaitUntilAsync(() => !Directory.Exists(this.directoryPath));

        // When:
        Directory.CreateDirectory(this.directoryPath);
        this.timeProvider.Advance(FileSystemDirectoryWatcher.RetryInterval);
        var changesWhenDirectoryAppeared = Volatile.Read(ref changeCount);
        await File.WriteAllTextAsync(Path.Combine(this.directoryPath, "Test.dll"), "changed");

        // Then:
        Assert.That(
            async () => await WaitUntilAsync(() => Volatile.Read(ref changeCount) > changesWhenDirectoryAppeared),
            Throws.Nothing);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeoutCts = new CancellationTokenSource(Timeout);
        while (!condition())
        {
            await Task.Delay(TimeSpan.FromMilliseconds(20), timeoutCts.Token);
        }
    }
}