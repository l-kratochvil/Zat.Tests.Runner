namespace Zat.Tests.Runner.WebApp.Shared.Services;

using Microsoft.AspNetCore.Components.Server.Circuits;

using Zat.Tests.Runner.Common.Net.Application.DependencyInjection;

/// <summary>
/// Initialises the scoped services of a circuit as soon as the circuit opens, before any of its
/// components render.
/// </summary>
/// <remarks>
/// Prerendering runs in a request scope of its own, which this does not reach.
/// </remarks>
/// <param name="provider">Service provider of the circuit's scope.</param>
/// <param name="services">Registrations the initialisable services are looked for in.</param>
internal sealed class ScopedServicesInitializer(IServiceProvider provider, IServiceCollection services)
    : CircuitHandler
{
    /// <inheritdoc/>
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        provider.InitInitializableServices(services, ServiceLifetime.Scoped);

        return Task.CompletedTask;
    }
}