namespace Zat.Tests.Runner.WebApp.Tests.Application.Logging;

using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.Logging;
using Zat.Tests.Runner.WebApp.Application.Paths;

[TestFixture]
public class FileLoggerProviderTests
{
    private const string GivenCategory = "Zat.Tests.Runner.TuiAppLog.App";

    private string directoryPath;

    [SetUp]
    public void SetUp()
    {
        this.directoryPath = Path.Combine(Path.GetTempPath(), $"filelogger-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.directoryPath))
        {
            Directory.Delete(this.directoryPath, recursive: true);
        }
        else if (File.Exists(this.directoryPath))
        {
            File.Delete(this.directoryPath);
        }
    }

    [Test]
    public void CreateLogger__WhenAnEntryIsLogged__ThenShouldWriteItIntoTodaysFile()
    {
        // Given:
        const string givenMessage = "Application started.";

        // When:
        this.LogAndFlush(logger => logger.LogInformation("{Message}", givenMessage));

        // Then:
        var lines = this.ReadTodaysFile();
        Assert.That(
            lines.Single(),
            Does.Match(
                @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \[INFO \] \["
                + Regex.Escape(GivenCategory)
                + @"\] "
                + Regex.Escape(givenMessage)
                + "$"));
    }

    [TestCase(LogLevel.Trace, "TRACE")]
    [TestCase(LogLevel.Debug, "DEBUG")]
    [TestCase(LogLevel.Information, "INFO ")]
    [TestCase(LogLevel.Warning, "WARN ")]
    [TestCase(LogLevel.Error, "ERROR")]
    [TestCase(LogLevel.Critical, "CRIT ")]
    public void CreateLogger__WhenAnEntryIsLoggedAtALevel__ThenShouldWriteThatLevelInAFixedWidthColumn(
        LogLevel givenLevel, string expectedName)
    {
        // When:
        this.LogAndFlush(logger => logger.Log(givenLevel, "{Message}", "message"));

        // Then:
        Assert.That(this.ReadTodaysFile().Single(), Does.Contain($"[{expectedName}] "));
    }

    [Test]
    public void CreateLogger__WhenTheMessageSpansSeveralLines__ThenShouldIndentEveryLineButTheFirst()
    {
        // Given:
        string[] expectedLines =
        [
            "Test run finished.",
            "    line 1",
            "    line 2",
        ];

        // When:
        this.LogAndFlush(logger =>
            logger.LogInformation("{Message}\n{Detail}", "Test run finished.", "line 1\nline 2"));

        // Then:
        var lines = this.ReadTodaysFile();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines, Has.Length.EqualTo(expectedLines.Length));
            Assert.That(lines[0], Does.EndWith(expectedLines[0]));
            Assert.That(lines[1], Is.EqualTo(expectedLines[1]));
            Assert.That(lines[2], Is.EqualTo(expectedLines[2]));
        }
    }

    [Test]
    public void CreateLogger__WhenAnExceptionIsAttached__ThenShouldWriteItAsIndentedDetail()
    {
        // Given:
        var givenException = new InvalidOperationException("boom");

        // When:
        this.LogAndFlush(logger => logger.LogError(givenException, "{Message}", "failed"));

        // Then:
        var lines = this.ReadTodaysFile();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[0], Does.EndWith("failed"));
            Assert.That(lines[1], Does.StartWith("    ").And.Contains("boom"));
        }
    }

    [Test]
    public void CreateLogger__WhenTheMessageCarriesProperties__ThenShouldWriteThemRenderedIntoTheLine()
    {
        // Given:
        // Structured records are rendered the way the logging pipeline formats them, with the
        // values in place and unquoted: the file is read by people, not parsed.
        const string expectedMessage = "Discovered 3 tests in suite.dll.";

        // When:
        this.LogAndFlush(logger =>
            logger.LogInformation("Discovered {Count} tests in {Assembly}.", 3, "suite.dll"));

        // Then:
        Assert.That(this.ReadTodaysFile().Single(), Does.EndWith(expectedMessage));
    }

    [Test]
    public void CreateLogger__WhenTheEntryIsWrittenInsideAScope__ThenShouldKeepTheScopeOutOfTheFile()
    {
        // When:
        this.LogAndFlush(logger =>
        {
            using (logger.BeginScope("scope {Secret}", "unwanted"))
            {
                logger.LogInformation("{Message}", "message");
            }
        });

        // Then:
        Assert.That(
            this.ReadTodaysFile().Single(),
            Does.EndWith("message").And.Not.Contains("unwanted"));
    }

    [Test]
    public void CreateLogger__WhenTodaysFileAlreadyHasRecords__ThenShouldAppendToThem()
    {
        // Given:
        // Restarting the application must not cost the day what it has already written.
        const string givenExistingLine = "2026-01-01 00:00:00.000 [INFO ] [Earlier.Run] earlier";

        Directory.CreateDirectory(this.directoryPath);
        File.WriteAllLines(
            LogFile.GetPath(this.directoryPath, DateTimeOffset.Now), [givenExistingLine]);

        // When:
        this.LogAndFlush(logger => logger.LogInformation("{Message}", "later"));

        // Then:
        var lines = this.ReadTodaysFile();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[0], Is.EqualTo(givenExistingLine));
            Assert.That(lines[1], Does.EndWith("later"));
        }
    }

    [Test]
    public void CreateLogger__WhenTheSameCategoryIsAskedForTwice__ThenShouldReturnTheSameLogger()
    {
        // Given:
        using var unit = this.CreateUnit();

        // When:
        var result = unit.CreateLogger(GivenCategory);

        // Then:
        Assert.That(result, Is.SameAs(unit.CreateLogger(GivenCategory)));
    }

    [Test]
    public void CreateLogger__WhenOlderFilesExist__ThenShouldApplyRetentionOnTheFirstWrite()
    {
        // Given:
        // Retention runs on the first write and not in the constructor, so that a failure of it
        // can still reach a handler attached after the provider was created.
        Directory.CreateDirectory(this.directoryPath);
        string[] givenOldFileNames =
            ["2020-01-01.log", "2020-01-02.log", "2020-01-03.log", "2020-01-04.log", "2020-01-05.log"];

        foreach (var fileName in givenOldFileNames)
        {
            File.WriteAllText(Path.Combine(this.directoryPath, fileName), string.Empty);
        }

        // When:
        this.LogAndFlush(logger => logger.LogInformation("{Message}", "message"), retainedFileCount: 3);

        // Then:
        // Today's file counts as one of the retained files, so only the two newest old ones stay.
        Assert.That(
            Directory.EnumerateFiles(this.directoryPath).Select(path => Path.GetFileName(path)).Order(StringComparer.Ordinal),
            Is.EqualTo(new[]
            {
                "2020-01-04.log",
                "2020-01-05.log",
                Path.GetFileName(LogFile.GetPath(this.directoryPath, DateTimeOffset.Now)),
            }));
    }

    [Test]
    public void Dispose__WhenCalledTwice__ThenShouldNotThrow()
    {
        // Given:
        // The logger factory and the container both dispose the provider.
        var unit = this.CreateUnit();
        unit.CreateLogger(GivenCategory).LogInformation("{Message}", "message");

        // When:
        unit.Dispose();

        // Then:
        Assert.That(unit.Dispose, Throws.Nothing);
    }

    private static IAppPathsProvider CreatePaths(string logsPath)
    {
        var paths = new Mock<IAppPathsProvider>();
        paths.SetupGet(provider => provider.Directories)
            .Returns(new AppDirectoryPaths(logsPath, logsPath));

        return paths.Object;
    }

    private FileLoggerProvider CreateUnit(int retainedFileCount = 5)
    {
        return new FileLoggerProvider(
            Options.Create(new FileLoggerOptions
            {
                RetainedFileCount = retainedFileCount,
            }),
            CreatePaths(string.Empty));
    }

    /// <summary>
    /// Logs through the provider and disposes it, because disposing is what flushes the pending
    /// records to disk.
    /// </summary>
    private void LogAndFlush(Action<ILogger> log, int retainedFileCount = 5)
    {
        using var unit = this.CreateUnit(retainedFileCount);
        log(unit.CreateLogger(GivenCategory));
    }

    private void BlockTheDirectory()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this.directoryPath)!);
        File.WriteAllText(this.directoryPath, string.Empty);
    }

    private string[] ReadTodaysFile()
    {
        return File.ReadAllLines(LogFile.GetPath(this.directoryPath, DateTimeOffset.Now));
    }
}