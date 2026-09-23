namespace Zat.Tests.Runner.WebApp.Tests.Application.Logging;

using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.DependencyInjection;
using Zat.Tests.Runner.WebApp.Application.Logging;

[TestFixture]
public class InitLoggingExtensionsTests
{
    private string dataPath;

    [SetUp]
    public void SetUp()
        => this.dataPath = Path.Combine(Path.GetTempPath(), $"filelogger-{Guid.NewGuid():N}");

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.dataPath))
        {
            Directory.Delete(this.dataPath, recursive: true);
        }
    }

    [Test]
    public void InitFileLogger__WhenTheLoggingSectionIsConfigured__ThenShouldBindTheOptions()
    {
        // Given:
        var givenPath = Path.Combine(this.dataPath, "elsewhere");

        // When:
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["App:LocalAppDataPath"] = this.dataPath,
            ["Logging:File:Path"] = givenPath,
            ["Logging:File:RetainedFileCount"] = "3",
        });
        var result = provider.GetRequiredService<IOptions<FileLoggerOptions>>().Value;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.RetainedFileCount, Is.EqualTo(3));
        }
    }

    [Test]
    public void InitFileLogger__WhenTheLoggingSectionIsSilent__ThenShouldKeepTheRetainedFileCount()
    {
        // When:
        using var provider = BuildProvider(
            new Dictionary<string, string?> { ["App:LocalAppDataPath"] = this.dataPath });
        var result = provider.GetRequiredService<IOptions<FileLoggerOptions>>().Value;

        // Then:
        Assert.That(result.RetainedFileCount, Is.EqualTo(5));
    }

    [Test]
    public void InitFileLogger__WhenNoLogPathIsConfigured__ThenShouldWriteUnderTheApplicationDataRoot()
    {
        // Given:
        var expectedPath = Path.Combine(this.dataPath, "logs");

        // When:
        WriteOneEntry(new Dictionary<string, string?> { ["App:LocalAppDataPath"] = this.dataPath });

        // Then:
        Assert.That(LogFile.Enumerate(expectedPath), Is.Not.Empty);
    }

    [Test]
    public void InitFileLogger__WhenTheLoggingSectionNamesALogPath__ThenShouldWriteThere()
    {
        // Given:
        var expectedPath = Path.Combine(this.dataPath, "elsewhere");

        // When:
        WriteOneEntry(new Dictionary<string, string?>
        {
            ["App:LocalAppDataPath"] = this.dataPath,
            ["Logging:File:Path"] = expectedPath,
        });

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(LogFile.Enumerate(expectedPath), Is.Not.Empty);
            Assert.That(LogFile.Enumerate(Path.Combine(this.dataPath, "logs")), Is.Empty);
        }
    }

    /// <summary>
    /// Logs one entry and shuts the container down, which is what drains the background writer.
    /// </summary>
    /// <param name="configuration">Configuration the container is built from.</param>
    private static void WriteOneEntry(Dictionary<string, string?> configuration)
    {
        using var provider = BuildProvider(configuration);

        provider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Zat.Tests.Runner.WebApp.Tests")
            .LogError("Entry.");
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> configuration)
    {
        IConfiguration builtConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(builtConfiguration);
        services.InitSharedServices();
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(builtConfiguration.GetSection("Logging"));
            builder.InitFileLogger();
        });

        return services.BuildServiceProvider();
    }
}