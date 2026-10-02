namespace Zat.Tests.Runner.WebApp.Tests.Application.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Application.Paths;

[TestFixture]
public class FileLoggerProviderTests
{
    private const string GivenCategory = "Zat.Tests.Runner.TuiAppLog.App";

    private string directoryPath;
    private FileLoggerProvider unit;

    [SetUp]
    public void SetUp()
    {
        this.directoryPath = Path.Combine(Path.GetTempPath(), $"filelogger-{Guid.NewGuid():N}");

        var paths = new Mock<ISharedAppPathsProvider>();
        paths.SetupGet(provider => provider.Directories)
            .Returns(new SharedAppDirectoryPaths(this.directoryPath, this.directoryPath));

        this.unit = new FileLoggerProvider(
            Options.Create(new FileLoggerOptions()),
            paths.Object);
    }

    [TearDown]
    public void TearDown()
    {
        this.unit.Dispose();

        if (Directory.Exists(this.directoryPath))
        {
            Directory.Delete(this.directoryPath, recursive: true);
        }
    }

    [Test]
    public void CreateLogger__WhenCalled__ThenShouldReturnLogger()
    {
        // When:
        var result = this.unit.CreateLogger(GivenCategory);

        // Then:
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void CreateLogger__WhenSameCategoryIsAskedForTwice__ThenShouldReturnSameLogger()
    {
        // Given:
        var expected = this.unit.CreateLogger(GivenCategory);

        // When:
        var result = this.unit.CreateLogger(GivenCategory);

        // Then:
        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void CreateLogger__WhenDifferentCategoryIsAskedFor__ThenShouldReturnDifferentLogger()
    {
        // Given:
        const string givenOtherCategory = "Zat.Tests.Runner.TuiAppLog.Other";
        var givenLogger = this.unit.CreateLogger(GivenCategory);

        // When:
        var result = this.unit.CreateLogger(givenOtherCategory);

        // Then:
        Assert.That(result, Is.Not.SameAs(givenLogger));
    }

    [Test]
    public void CreateLogger__WhenEntryIsLogged__ThenShouldWriteItIntoFileInGivenDirectory()
    {
        // Given:
        const string givenMessage = "Application started.";

        // When:
        this.unit.CreateLogger(GivenCategory).LogInformation("{Message}", givenMessage);

        // Disposing flushes the pending records to disk.
        this.unit.Dispose();

        // Then:
        Assert.That(
            Directory.EnumerateFiles(this.directoryPath).Select(File.ReadAllText),
            Has.One.Contains(givenMessage));
    }
}