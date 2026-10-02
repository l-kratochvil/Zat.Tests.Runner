namespace Zat.Tests.Runner.WebApp.Tests.Application.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

using NUnit.Framework;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Application.Paths;
using Zat.Tests.Runner.WebApp.Application.DependencyInjection;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Features.AppSettings.Services;
using Zat.Tests.Runner.WebApp.Shared.JsInterop;

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
    public void InitFeatures__WhenHostedServicesAreResolved__ThenShouldRegisterOnlySettingsStore()
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
    public void InitSharedServices__WhenJsModuleInteropFactoryIsRegistered__ThenShouldBeScoped()
    {
        // Given:
        // Scoped, because the JavaScript runtime it is built around belongs to a single circuit.
        // A singleton would call into the browser session of whoever resolved it first.
        var givenServices = new ServiceCollection();

        // When:
        givenServices.InitServices();

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

        var paths = CreatePaths(logsDirectoryPath);

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddLogging(builder => builder.InitLogging());
        services.Configure<FileLoggerOptions>(configuration);
        services.AddSingleton(paths);
        services.AddSingleton<ISharedAppPathsProvider>(paths);
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
            .Returns(new SharedAppDirectoryPaths(AppData: appDataPath, Logs: appDataPath));
        paths.SetupGet(provider => provider.Files)
            .Returns(new SharedAppFilePaths(
                UserSettings: Path.Combine(appDataPath, "user-settings.json"),
                MainAssemblyDll: Path.Combine(appDataPath, "main-assembly.dll")));

        return paths.Object;
    }
}