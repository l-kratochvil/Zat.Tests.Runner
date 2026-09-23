namespace Zat.Tests.Runner.WebApp.Tests.Application.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.DependencyInjection;
using Zat.Tests.Runner.WebApp.Application.Logging;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;
using Zat.Tests.Runner.WebApp.Features.AppSettings.Services;
using Zat.Tests.Runner.WebApp.Shared.JsInterop;
using Zat.Tests.Runner.WebApp.Shared.Logging;

[TestFixture]
public class InitServicesExtensionTests
{
    private string logsDirectoryPath;
    private ServiceProvider unit;

    [SetUp]
    public void SetUp()
    {
        this.logsDirectoryPath = Path.Combine(Path.GetTempPath(), $"applogging-di-{Guid.NewGuid():N}");
        this.unit = BuildProvider(this.logsDirectoryPath);
    }

    [TearDown]
    public void TearDown()
    {
        this.unit.Dispose();

        if (Directory.Exists(this.logsDirectoryPath))
        {
            Directory.Delete(this.logsDirectoryPath, recursive: true);
        }
    }

    [Test]
    public void InitAppLogging__WhenTheDefaultLoggerIsResolved__ThenShouldBeBoundToTheAppSource()
    {
        // When:
        var result = this.unit.GetRequiredService<IAppLogger>();

        // Then:
        Assert.That(result.Source, Is.EqualTo(LogSources.App));
    }

    [Test]
    public void InitAppLogging__WhenLoggersOfDifferentSourcesLog__ThenShouldAppendIntoTheSameStore()
    {
        // Given:
        string[] expectedSources = [LogSources.TestRun, LogSources.TestLink];
        var givenFactory = this.unit.GetRequiredService<IAppLoggerFactory>();

        // When:
        givenFactory.CreateLogger(expectedSources[0]).Warning("slow");
        givenFactory.CreateLogger(expectedSources[1]).Error("unreachable");

        // Then:
        Assert.That(
            this.unit.GetRequiredService<IAppLoggerHub>().GetEntries().Select(entry => entry.Source),
            Is.EqualTo(expectedSources));
    }

    [Test]
    public void InitAppLogging__WhenTheSinksAreResolved__ThenShouldRegisterTheBridgeIntoTheLoggingPipeline()
    {
        // Given:
        Type[] expectedTypes = [typeof(DiagnosticsLoggerSink)];

        // When:
        var result = this.unit.GetServices<IAppLoggerSink>().Select(sink => sink.GetType());

        // Then:
        Assert.That(result, Is.EqualTo(expectedTypes));
    }

    [Test]
    public void InitFeatures__WhenHostedServicesAreResolved__ThenShouldRegisterOnlyTheSettingsStore()
    {
        // Given:
        // The log file is owned by the logging pipeline, so it needs no lifecycle of its own. The
        // settings do: they are read from their file before the first browser is answered.
        Type[] expectedTypes = [typeof(AppSettingsStore)];

        // When:
        var result = this.unit.GetServices<IHostedService>().Select(service => service.GetType());

        // Then:
        Assert.That(result, Is.EqualTo(expectedTypes));
    }

    [Test]
    public void InitAppLogging__WhenAnEntryIsLogged__ThenShouldReachTheLogFileThroughTheLoggingPipeline()
    {
        // Given:
        const string givenMessage = "Test run finished.";
        this.unit.GetRequiredService<IAppLogger>().Info(givenMessage);

        // When:
        // Disposing flushes the pending records of the file provider.
        this.unit.Dispose();

        // Then:
        var content = File.ReadAllText(LogFile.GetPath(this.logsDirectoryPath, DateTimeOffset.Now));
        Assert.That(content, Does.Contain(DiagnosticsLoggerSink.GetCategory(LogSources.App)).And.Contains(givenMessage));
    }

    [Test]
    public void InitAppLogging__WhenTheLogFileCannotBeWritten__ThenShouldReportItInTheHub()
    {
        // Given:
        // A file where the logs directory should be, so that the provider cannot write anything.
        // A silently broken log file is the worst way for a log to fail, so it has to surface in
        // the panel.
        var givenBlockedPath = Path.Combine(Path.GetTempPath(), $"applogging-di-blocked-{Guid.NewGuid():N}");
        File.WriteAllText(givenBlockedPath, string.Empty);

        try
        {
            var provider = BuildProvider(givenBlockedPath);
            var loggerHub = provider.GetRequiredService<IAppLoggerHub>();

            // When:
            provider.GetRequiredService<IAppLogger>().Info("message");
            provider.Dispose();

            // Then:
            Assert.That(
                loggerHub.GetEntries().Select(entry => entry.Message),
                Has.Some.Contains("log file"));
        }
        finally
        {
            File.Delete(givenBlockedPath);
        }
    }

    [Test]
    public void InitSharedServices__WhenTheJsModuleInteropFactoryIsRegistered__ThenShouldBeScoped()
    {
        // Given:
        // Scoped, because the JavaScript runtime it is built around belongs to a single circuit.
        // A singleton would call into the browser session of whoever resolved it first.
        var givenServices = new ServiceCollection();

        // When:
        givenServices.InitSharedServices();

        // Then:
        var result = givenServices.Single(
            descriptor => descriptor.ServiceType == typeof(IJsModuleInteropFactory));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(result.ImplementationType, Is.EqualTo(typeof(JsModuleInteropFactory)));
        }
    }

    private static ServiceProvider BuildProvider(string logsDirectoryPath)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Path"] = logsDirectoryPath,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddLogging(builder => builder.InitFileLogger());
        services.Configure<FileLoggerOptions>(configuration);
        services.AddSingleton(CreatePaths(logsDirectoryPath));
        services.InitFeatures();

        return services.BuildServiceProvider();
    }

    // Where the application keeps its files is the paths provider's own business, tested in its own
    // fixture. Standing it in here keeps this fixture about the registrations it names, and puts
    // the log file straight into the temporary directory the fixture owns and deletes.
    private static IAppPathsProvider CreatePaths(string appDataPath)
    {
        var paths = new Mock<IAppPathsProvider>();

        paths.SetupGet(provider => provider.Directories)
            .Returns(new AppDirectoryPaths(AppData: appDataPath, Logs: appDataPath));
        paths.SetupGet(provider => provider.Files)
            .Returns(new AppFilePaths(
                UserSettings: Path.Combine(appDataPath, "user-settings.json"),
                MainAssemblyDll: Path.Combine(appDataPath, "main-assembly.dll")));

        return paths.Object;
    }
}