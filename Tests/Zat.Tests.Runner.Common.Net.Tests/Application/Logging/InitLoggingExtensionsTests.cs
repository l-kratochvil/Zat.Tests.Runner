namespace Zat.Tests.Runner.Common.Net.Tests.Application.Logging;

using System.Collections.Concurrent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Application.Paths;
using Zat.Tests.Runner.Common.Net.Extensions;

[TestFixture]
public class InitLoggingExtensionsTests
{
    private const string GivenFrameworkCategory = "Microsoft.AspNetCore.Hosting.Diagnostics";

    private string directoryPath;
    private ServiceProvider unit;

    private AppLoggerProvider AppLogs
        => this.unit.GetRequiredService<AppLoggerProvider>();

    private DefaultLoggerProvider DefaultLogs
        => this.unit.GetRequiredService<DefaultLoggerProvider>();

    [SetUp]
    public void SetUp()
    {
        this.directoryPath = Path.Combine(Path.GetTempPath(), $"initlogging-{Guid.NewGuid():N}");

        var services = CreateServices(this.directoryPath);
        services
            .AddSingletonLoggerProvider<DefaultLoggerProvider>()
            .AddSingletonAppLoggerProvider<AppLoggerProvider>();

        this.unit = services.BuildServiceProvider();
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
    public void InitLogging__WhenLoggedUsingLogSourcesAppLogger__ThenShouldUseAppLoggerProvider()
    {
        // When:
        this.unit.GetRequiredService<ILogger<LogSources.App>>().LogInformation("app");

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.AppLogs.Messages, Is.EqualTo(["app"]));
            Assert.That(this.DefaultLogs.Messages, Is.Empty);
        }
    }

    [Test]
    public void InitLogging__WhenLoggedUsingLoggerOtherThanLogSourcesApp__ThenShouldUseDefaultLoggerProvider()
    {
        // When:
        this.unit.GetRequiredService<ILogger>().LogInformation("plain");
        this.unit.GetRequiredService<ILogger<LogSources.TestRun>>().LogInformation("test-run");
        this.unit.GetRequiredService<ILoggerFactory>()
            .CreateLogger(GivenFrameworkCategory)
            .LogInformation("framework");

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                this.DefaultLogs.Messages,
                Is.EqualTo(["plain", "test-run", "framework"]));
            Assert.That(this.AppLogs.Messages, Is.Empty);
        }
    }

    [Test]
    public void InitLogging__WhenPlainLoggerIsUsed__ThenShouldWriteUnderDefaultCategory()
    {
        // Given:
        var expected = typeof(LogSources.Default).FullName!.Replace('+', '.');

        // When:
        this.unit.GetRequiredService<ILogger>().LogInformation("plain");

        // Then:
        Assert.That(this.DefaultLogs.Categories, Is.EqualTo([expected]));
    }

    [Test]
    public void InitLogging__WhenNoLogProviderIsRegistered__ThenShouldStillResolveAppLogger()
    {
        // Given:
        // The TuiApp shows no log yet, yet shares services that write to it.
        using var provider = CreateServices(this.directoryPath).BuildServiceProvider();

        // When:
        var logger = provider.GetRequiredService<ILogger<LogSources.App>>();

        // Then:
        Assert.That(() => logger.LogInformation("app"), Throws.Nothing);
    }

    private static ServiceCollection CreateServices(string directoryPath)
    {
        var paths = new Mock<ISharedAppPathsProvider>();
        paths.SetupGet(provider => provider.Directories)
            .Returns(new SharedAppDirectoryPaths(directoryPath, directoryPath));

        var services = new ServiceCollection();
        services.AddSingleton(paths.Object);
        services.AddLogging(builder => builder
            .AddConfiguration(new ConfigurationBuilder().Build())
            .InitLogging());
        services.InitLogging();

        return services;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class DefaultLoggerProvider : LoggerProviderBase;

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class AppLoggerProvider : LoggerProviderBase;

    private abstract class LoggerProviderBase : ILoggerProvider
    {
        private readonly ConcurrentQueue<(string Category, string Message)> records = new();

        public IEnumerable<string> Messages
            => this.records.Select(record => record.Message);

        public IEnumerable<string> Categories
            => this.records.Select(record => record.Category);

        public ILogger CreateLogger(string categoryName)
            => new Logger(categoryName, this.records);

        public void Dispose()
        {
        }

        private sealed class Logger(
            string categoryName,
            ConcurrentQueue<(string Category, string Message)> records)
            : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
                where TState : notnull
                => NullLogger.Instance.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel)
                => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
                => records.Enqueue((categoryName, formatter(state, exception)));
        }
    }
}
