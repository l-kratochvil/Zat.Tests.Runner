namespace Zat.Tests.Runner.WebApp.Shared.Services;

using CommunityToolkit.Mvvm.Messaging;

using Fluxor;

using Microsoft.AspNetCore.Components.Server.Circuits;

using Zat.Tests.Runner.Common.Net.Application.DependencyInjection;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

/// <summary>
/// Prepares a circuit as soon as it opens, before any of its components render: initialises its
/// scoped services and forwards the log, which is written outside of any circuit, to its store.
/// </summary>
/// <remarks>
/// Prerendering runs in a request scope of its own, which this does not reach.
/// </remarks>
/// <param name="provider">Service provider of the circuit's scope.</param>
/// <param name="services">Registrations the initialisable services are looked for in.</param>
/// <param name="globalMessanger">Messenger the log is announced on to every circuit.</param>
/// <param name="dispatcher">Dispatcher of the circuit's store.</param>
internal sealed class CircuitInitializer(
    IServiceProvider provider,
    IServiceCollection services,
    GlobalWeakReferenceMessanger globalMessanger,
    IDispatcher dispatcher)
    : CircuitHandler,
      IRecipient<DataChangedAction>,
      IDisposable
{
    /// <inheritdoc/>
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        provider.InitInitializableServices(services, ServiceLifetime.Scoped);
        globalMessanger.Register(this);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Receive(DataChangedAction message)
        => dispatcher.Dispatch(message);

    /// <inheritdoc/>
    public void Dispose()
        => globalMessanger.UnregisterAll(this);
}