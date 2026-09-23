namespace Zat.Tests.Runner.WebApp.Tests.Application.Paths;

using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.DependencyInjection;
using Zat.Tests.Runner.WebApp.Application.Paths;

[TestFixture]
public class InitPathsExtensionsTests
{
    private string dataPath;
    private ServiceCollection services = [];

    [SetUp]
    public void SetUp()
        => this.dataPath = Path.Combine(Path.GetTempPath(), $"apppaths-{Guid.NewGuid():N}");

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.dataPath))
        {
            Directory.Delete(this.dataPath, recursive: true);
        }
    }

    [Test]
    public void InitAppPaths__WhenTheApplicationDataPathIsConfigured__ThenShouldDeriveTheDirectoriesFromIt()
    {
        // When:
        using var provider = this.BuildProvider(this.dataPath);
        var result = provider.GetRequiredService<IAppPathsProvider>();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Directories.AppData, Is.EqualTo(this.dataPath));
            Assert.That(result.Directories.Logs, Is.EqualTo(Path.Combine(this.dataPath, "logs")));
        }
    }

    [Test]
    public void InitAppPaths__WhenTheApplicationDataPathIsConfigured__ThenShouldDeriveTheFilesFromIt()
    {
        // When:
        using var provider = this.BuildProvider(this.dataPath);
        var result = provider.GetRequiredService<IAppPathsProvider>();

        // Then:
        Assert.That(result.Files.UserSettings, Is.EqualTo(Path.Combine(this.dataPath, "user-settings.json")));
    }

    [Test]
    public void InitAppPaths__WhenNothingIsConfigured__ThenShouldRefuseToHandOutAnyPath()
    {
        // Given:
        // Where the application writes is answered by configuration alone, so a missing section is
        // a broken installation rather than something to guess around.
        using var provider = this.BuildProvider(configuredDataPath: null);

        // Then:
        Assert.That(
            provider.GetRequiredService<IAppPathsProvider>,
            Throws.InstanceOf<OptionsValidationException>());
    }

    [Test]
    public void InitAppPaths__WhenTheApplicationDataPathNamesAnEnvironmentVariable__ThenShouldExpandIt()
    {
        // Given:
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Elsewhere");

        // When:
        using var provider = this.BuildProvider(@"%LOCALAPPDATA%\Elsewhere");
        var result = provider.GetRequiredService<IAppPathsProvider>();

        // Then:
        Assert.That(result.Directories.AppData, Is.EqualTo(expectedPath));
    }

    [Test]
    public void InitAppPaths__WhenThePathsAreRead__ThenShouldCreateNothingOnDisk()
    {
        // When:
        using var provider = this.BuildProvider(this.dataPath);
        var paths = provider.GetRequiredService<IAppPathsProvider>();
        _ = paths.Directories.Logs;
        _ = paths.Files.UserSettings;

        // Then:
        Assert.That(Directory.Exists(paths.Directories.AppData), Is.False);
    }

    [Test]
    public void InitAppPaths__WhenTheServicesAreInitialised__ThenShouldCreateTheDirectories()
    {
        // Given:
        using var provider = this.BuildProvider(this.dataPath);
        var paths = provider.GetRequiredService<IAppPathsProvider>();

        // When:
        provider.InitInitializableServices(this.services);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Directory.Exists(paths.Directories.AppData), Is.True);
            Assert.That(Directory.Exists(paths.Directories.Logs), Is.True);
        }
    }

    [Test]
    public void InitAppPaths__WhenThePathsAreRegistered__ThenShouldRegisterThemOnce()
    {
        // Given:
        // One registration is all the paths need: what asks for initialisation says so by
        // implementing it, see InitDependencyInjectionExtensions.
        using var provider = this.BuildProvider(this.dataPath);

        // Then:
        Assert.That(
            this.services.Where(descriptor => descriptor.ImplementationType == typeof(AppPathsProvider)),
            Has.Exactly(1).Items);
    }

    [Test]
    public void InitAppPaths__WhenTheShippedSettingsAreRead__ThenShouldHoldTheApplicationDataPath()
    {
        // Given:
        // Nothing is defaulted in code any more, so the settings file shipped with the application
        // is the only thing standing between it and a refusal to start.
        IConfiguration givenConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var givenServices = new ServiceCollection();
        givenServices.AddSingleton(givenConfiguration);

        using var provider = givenServices.BuildServiceProvider();

        // When:
        var result = provider.GetRequiredService<IAppPathsProvider>();

        // Then:
        Assert.That(
            result.Directories.AppData,
            Is.EqualTo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Zat.Zat.Tests.Runner")));
    }

    [Test]
    public Task InitAppPaths__WhenTheApplicationSectionHoldsAnUnknownKey__ThenShouldNotStart()
    {
        // Given:
        // A key nobody reads is a setting that silently did nothing, which is worse than a refusal.
        var givenConfiguration = new Dictionary<string, string?>
        {
            ["App:LocalAppDataPath"] = this.dataPath,
            ["App:LclAppDataPath"] = this.dataPath,
        };

        // Then:
        return Assert.ThatAsync(
            () => StartHostAsync(givenConfiguration),
            Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("LclAppDataPath"));
    }

    [Test]
    public Task InitAppPaths__WhenTheApplicationDataPathIsEmpty__ThenShouldNotStart()
    {
        // Then:
        return Assert.ThatAsync(
            () => StartHostAsync(new Dictionary<string, string?> { ["App:LocalAppDataPath"] = string.Empty }),
            Throws.InstanceOf<OptionsValidationException>());
    }

    [Test]
    public Task InitAppPaths__WhenTheApplicationDataPathIsRelative__ThenShouldNotStart()
    {
        // Then:
        // Relative to what is a question nobody asking for a data folder wants to answer.
        return Assert.ThatAsync(
            () => StartHostAsync(new Dictionary<string, string?> { ["App:LocalAppDataPath"] = @"data\Zat.Tests.Runner" }),
            Throws.InstanceOf<OptionsValidationException>());
    }

    [Test]
    public async Task InitAppPaths__WhenTheApplicationDataPathIsAbsolute__ThenShouldStart()
    {
        // When:
        await StartHostAsync(new Dictionary<string, string?> { ["App:LocalAppDataPath"] = this.dataPath });

        // Then:
        Assert.That(Directory.Exists(this.dataPath), Is.False);
    }

    private static async Task StartHostAsync(Dictionary<string, string?> configuration)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        builder.Configuration.AddInMemoryCollection(configuration);
        builder.Services.InitSharedServices();

        using var host = builder.Build();
        await host.StartAsync();
        await host.StopAsync();
    }

    private ServiceProvider BuildProvider(string? configuredDataPath)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                configuredDataPath is null
                    ? []
                    : new Dictionary<string, string?> { ["App:LocalAppDataPath"] = configuredDataPath })
            .Build();

        this.services = [];
        this.services.AddSingleton(configuration);
        this.services.InitSharedServices();

        return this.services.BuildServiceProvider();
    }
}