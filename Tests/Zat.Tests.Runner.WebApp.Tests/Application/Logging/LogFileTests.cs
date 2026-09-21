namespace Zat.Tests.Runner.WebApp.Tests.Application.Logging;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.Logging;

[TestFixture]
public class LogFileTests
{
    private string directoryPath;

    [SetUp]
    public void SetUp()
    {
        this.directoryPath = Path.Combine(Path.GetTempPath(), $"logfile-{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.directoryPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.directoryPath))
        {
            Directory.Delete(this.directoryPath, recursive: true);
        }
    }

    [Test]
    public void GetPath__WhenAskedForAMoment__ThenShouldNameTheFileAfterThatDay()
    {
        // Given:
        var givenTimestamp = new DateTimeOffset(2026, 8, 27, 23, 59, 59, TimeSpan.Zero);

        // When:
        var result = LogFile.GetPath(this.directoryPath, givenTimestamp);

        // Then:
        Assert.That(Path.GetFileName(result), Is.EqualTo("2026-08-27.log"));
    }

    [Test]
    public void Enumerate__WhenTheDirectoryDoesNotExist__ThenShouldReturnNothing()
    {
        // When:
        var result = LogFile.Enumerate(Path.Combine(this.directoryPath, "missing"));

        // Then:
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Enumerate__WhenTheDirectoryHoldsForeignFiles__ThenShouldReturnOnlyTheLogFiles()
    {
        // Given:
        // These names are the trap the date parsing exists for: a wildcard would match them too.
        this.CreateFiles("2026-08-26.log", "2026-08-27.log", "2026-08-27.log2", "notes.log", "2026-08-27.txt");

        // When:
        var result = LogFile.Enumerate(this.directoryPath);

        // Then:
        Assert.That(
            result.Select(Path.GetFileName),
            Is.EqualTo(new[] { "2026-08-27.log", "2026-08-26.log" }));
    }

    [Test]
    public void Enumerate__WhenSeveralLogFilesExist__ThenShouldReturnThemNewestFirst()
    {
        // Given:
        this.CreateFiles("2026-08-26.log", "2026-09-01.log", "2025-12-31.log");

        // When:
        var result = LogFile.Enumerate(this.directoryPath);

        // Then:
        Assert.That(
            result.Select(Path.GetFileName),
            Is.EqualTo(new[] { "2026-09-01.log", "2026-08-26.log", "2025-12-31.log" }));
    }

    [Test]
    public void ApplyRetention__WhenMoreFilesExistThanRetained__ThenShouldKeepTheNewestOnes()
    {
        // Given:
        this.CreateFiles("2026-08-24.log", "2026-08-25.log", "2026-08-26.log", "2026-08-27.log");

        // When:
        LogFile.ApplyRetention(this.directoryPath, retainedFileCount: 2);

        // Then:
        Assert.That(this.GetFileNames(), Is.EqualTo(new[] { "2026-08-26.log", "2026-08-27.log" }));
    }

    [Test]
    public void ApplyRetention__WhenForeignFilesArePresent__ThenShouldLeaveThemAlone()
    {
        // Given:
        this.CreateFiles("2026-08-26.log", "2026-08-27.log", "notes.log", "2026-08-27.log2");

        // When:
        LogFile.ApplyRetention(this.directoryPath, retainedFileCount: 1);

        // Then:
        Assert.That(
            this.GetFileNames(),
            Is.EqualTo(new[] { "2026-08-27.log", "2026-08-27.log2", "notes.log" }));
    }

    [Test]
    public void ApplyRetention__WhenFewerFilesExistThanRetained__ThenShouldDeleteNothing()
    {
        // Given:
        this.CreateFiles("2026-08-26.log", "2026-08-27.log");

        // When:
        LogFile.ApplyRetention(this.directoryPath, retainedFileCount: 5);

        // Then:
        Assert.That(this.GetFileNames(), Has.Length.EqualTo(2));
    }

    [Test]
    public void ApplyRetention__WhenTheCountIsNegative__ThenShouldThrow()
    {
        // Then:
        Assert.That(
            () => LogFile.ApplyRetention(this.directoryPath, retainedFileCount: -1),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    private void CreateFiles(params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            File.WriteAllText(Path.Combine(this.directoryPath, fileName), string.Empty);
        }
    }

    private string[] GetFileNames()
    {
        return
        [
            .. Directory
                .EnumerateFiles(this.directoryPath)
                .Select(path => Path.GetFileName(path))
                .Order(StringComparer.Ordinal),
        ];
    }
}