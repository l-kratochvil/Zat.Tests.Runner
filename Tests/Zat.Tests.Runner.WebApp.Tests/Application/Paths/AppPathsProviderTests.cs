namespace Zat.Tests.Runner.WebApp.Tests.Application.Paths;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.DependencyInjection;
using Zat.Tests.Runner.WebApp.Application.Paths;

[TestFixture]
public class AppPathsProviderTests
{
    [Test]
    public void InitAppPaths__WhenShippedSettingsAreRead__ThenShouldHoldApplicationDataPath()
    {
        // Given:
        // Nothing is defaulted in code any more, so the settings file shipped with the application
        // is the only thing standing between it and a refusal to start.
        IConfiguration givenConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var givenServices = new ServiceCollection();
        givenServices.AddSingleton(givenConfiguration);
        givenServices.InitServices();

        using var provider = givenServices.BuildServiceProvider();

        // When:
        var result = provider.GetRequiredService<IAppPathsProvider>();

        // Then:
        Assert.That(
            result.Directories.AppData,
            Is.EqualTo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Zat.Tests.Runner.WebApp")));
    }
}