namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// The watcher runs against a real directory, as what it adapts is the file system itself.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class FileSystemDirectoryWatcherTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly string directoryPath = Path.Combine(Path.GetTempPath(), $"{nameof(FileSystemDirectoryWatcherTests)}-{Guid.NewGuid():N}");

    [SetUp]
    public void SetUp()
        => Directory.CreateDirectory(Path.Combine(this.directoryPath, "Sub"));

    [TearDown]
    public void TearDown()
        => Directory.Delete(this.directoryPath, recursive: true);

    [Test]
    public async Task Watch__WhenFileInSubdirectoryIsWritten__ThenShouldTellAboutIt()
    {
        // Given:
        var changed = new TaskCompletionSource();
        using var watching = new FileSystemDirectoryWatcher().Watch(this.directoryPath, () => changed.TrySetResult());

        // When:
        await File.WriteAllTextAsync(Path.Combine(this.directoryPath, "Sub", "Test.dll"), "changed");

        // Then:
        Assert.That(async () => await changed.Task.WaitAsync(Timeout), Throws.Nothing);
    }

    [Test]
    public async Task Watch__WhenStopped__ThenShouldNoLongerTellAboutChanges()
    {
        // Given:
        var changeCount = 0;
        var watching = new FileSystemDirectoryWatcher().Watch(this.directoryPath, () => Interlocked.Increment(ref changeCount));

        // When:
        watching.Dispose();
        await File.WriteAllTextAsync(Path.Combine(this.directoryPath, "Test.dll"), "changed");
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        // Then:
        Assert.That(changeCount, Is.Zero);
    }
}