namespace Zat.Tests.Runner.WebApp.Tests.Shared.Services;

using DevKit.Core.Interfaces;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

[TestFixture]
public class CircuitInitializerTests
{
    private ServiceCollection services;
    private ServiceProvider rootProvider;
    private IServiceScope circuitScope;
    private GlobalWeakReferenceMessanger globalMessanger;
    private Mock<IDispatcher> dispatcherMock;
    private CircuitInitializer unit;

    [SetUp]
    public void SetUp()
    {
        this.services = new ServiceCollection();
        this.services.AddScoped<InitializableService>();
        this.rootProvider = this.services.BuildServiceProvider();
        this.circuitScope = this.rootProvider.CreateScope();
        this.globalMessanger = new GlobalWeakReferenceMessanger();
        this.dispatcherMock = new Mock<IDispatcher>();

        this.unit = this.CreateUnit();
    }

    [TearDown]
    public void TearDown()
    {
        this.unit.Dispose();
        this.circuitScope.Dispose();
        this.rootProvider.Dispose();
    }

    [Test]
    public async Task OnCircuitOpenedAsync__WhenGlobalLogMessageIsSent__ThenShouldDispatchItToCircuit()
    {
        // Given:
        var givenAction = new DataChangedAction(null);
        await this.unit.OnCircuitOpenedAsync(null!, CancellationToken.None);

        // When:
        this.globalMessanger.Send(givenAction);

        // Then:
        this.dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(givenAction), Times.Once);
    }

    [Test]
    public async Task Dispose__WhenGlobalLogMessageIsSentAfterwards__ThenShouldNotDispatchIt()
    {
        // Given:
        // The circuit is gone, so its store must not be fed any more.
        await this.unit.OnCircuitOpenedAsync(null!, CancellationToken.None);
        this.unit.Dispose();

        // When:
        this.globalMessanger.Send(new DataChangedAction(null));

        // Then:
        this.dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task OnCircuitOpenedAsync__WhenScopedServiceIsInitializable__ThenShouldInitializeIt()
    {
        // When:
        await this.unit.OnCircuitOpenedAsync(null!, CancellationToken.None);

        // Then:
        var service = this.circuitScope.ServiceProvider.GetRequiredService<InitializableService>();
        Assert.That(service.IsInitialized, Is.True);
    }

    private CircuitInitializer CreateUnit()
        => new(
            this.circuitScope.ServiceProvider,
            this.services,
            this.globalMessanger,
            this.dispatcherMock.Object);

    private sealed class InitializableService : IInitializable
    {
        public bool IsInitialized { get; private set; }

        public void Initialize()
            => this.IsInitialized = true;
    }
}