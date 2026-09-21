namespace Zat.Tests.Runner.WebApp.Tests.Application.DependencyInjection;

using DevKit.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.DependencyInjection;

/// <summary>
/// Which of the registered services are initialised, and how often.
/// </summary>
[TestFixture]
public class InitDependencyInjectionExtensionsTests
{
    private interface IService;

    [Test]
    public void InitDependencyInjection__WhenAServiceImplementsTheInterface__ThenShouldInitialiseItWithoutASecondRegistration()
    {
        // Given:
        // Asking for initialisation is what implementing the interface means, so nothing else has
        // to be said at the registration.
        var services = new ServiceCollection();
        services.AddSingleton<IService, Service>();

        using var provider = services.BuildServiceProvider();

        // When:
        provider.InitInitializableServices(services);

        // Then:
        Assert.That(((Service)provider.GetRequiredService<IService>()).InitializeCount, Is.EqualTo(1));
    }

    [Test]
    public void InitDependencyInjection__WhenOneServiceIsRegisteredUnderSeveralTypes__ThenShouldInitialiseItOnce()
    {
        // Given:
        var services = new ServiceCollection();
        services.AddSingleton<Service>();
        services.AddSingleton<IService>(provider => provider.GetRequiredService<Service>());

        using var provider = services.BuildServiceProvider();

        // When:
        provider.InitInitializableServices(services);

        // Then:
        Assert.That(provider.GetRequiredService<Service>().InitializeCount, Is.EqualTo(1));
    }

    [Test]
    public void InitDependencyInjection__WhenAServiceDoesNotAskForIt__ThenShouldLeaveItAlone()
    {
        // Given:
        // Initialising is not resolving: a service that never said it needs it is not built here
        // either.
        var services = new ServiceCollection();
        services.AddSingleton<PlainService>();

        using var provider = services.BuildServiceProvider();

        // When:
        provider.InitInitializableServices(services);

        // Then:
        Assert.That(PlainService.ConstructedCount, Is.Zero);
    }

    [Test]
    public void InitDependencyInjection__WhenAServiceIsRegisteredAsAnOpenGeneric__ThenShouldPassItBy()
    {
        // Given:
        // The logging pipeline registers ILogger<> that way, and an open generic cannot be
        // resolved at all, so reaching for it would break every startup.
        var services = new ServiceCollection();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();

        // Then:
        Assert.That(() => provider.InitInitializableServices(services), Throws.Nothing);
    }

    [Test]
    public void InitDependencyInjection__WhenAnInitialisableServiceIsScoped__ThenShouldSkipItRatherThanFail()
    {
        // Given:
        // A scoped service belongs to a circuit that does not exist yet at startup, so there is
        // nothing to initialise here.
        var services = new ServiceCollection();
        services.AddScoped<IService, Service>();

        using var provider = services.BuildServiceProvider();

        // Then:
        Assert.That(() => provider.InitInitializableServices(services), Throws.Nothing);
    }

    private sealed class Service : IService, IInitializable
    {
        public int InitializeCount { get; private set; }

        public void Initialize()
            => this.InitializeCount++;
    }

    private sealed class PlainService
    {
        public PlainService()
            => ConstructedCount++;

        public static int ConstructedCount { get; private set; }
    }
}