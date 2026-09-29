namespace Zat.Tests.Runner.WebApp.Application.DependencyInjection;

using DevKit.Core.Extensions.Collections;
using DevKit.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Initialisation of the services that ask for it.
/// </summary>
public static class CoreExtensions
{
    /// <param name="provider">Service provider to extend.</param>
    extension(IServiceProvider provider)
    {
        /// <summary>
        /// Initialises every registered singleton implementing <see cref="IInitializable"/>.
        /// </summary>
        /// <remarks>
        /// The registrations are read rather than a second one asked for, so a service says it
        /// needs initialising by implementing the interface and nowhere else. A registration whose
        /// implementation is a factory delegate hides its type and is therefore not seen: an
        /// initialisable service is registered by its type.
        /// <para>
        /// What each of them does is their own business; the composition root only says when.
        /// </para>
        /// </remarks>
        /// <param name="services">Registrations the initialisable services are looked for in.</param>
        public void InitInitializableServices(IServiceCollection services)
        {
            var initialized = new HashSet<object>(ReferenceEqualityComparer.Instance);

            foreach (var service in services
                         .Where(static descriptor => descriptor.Lifetime is ServiceLifetime.Singleton
                                                     && !descriptor.IsKeyedService
                                                     && IsInitializable(descriptor))
                         .Select(descriptor => provider.GetRequiredService(descriptor.ServiceType))
                         .Select(service => service as IInitializable)
                         .WhereNotNull())
            {
                if (initialized.Add(service))
                {
                    service.Initialize();
                }
            }
        }
    }

    /// <summary>
    /// Tells whether <paramref name="descriptor"/> registers an <see cref="IInitializable"/>.
    /// </summary>
    /// <remarks>
    /// Read from the registration rather than from the service, so that resolving it is what
    /// initialising costs and not what looking for it does. An open generic — <c>ILogger&lt;&gt;</c>
    /// and the like — cannot be resolved by its unbound service type at all, so it is turned away
    /// here rather than crashing the very scan meant to find what needs initialising.
    /// </remarks>
    /// <param name="descriptor">Registration to read.</param>
    /// <returns><see langword="true"/> when the registered implementation asks to be initialised.</returns>
    private static bool IsInitializable(ServiceDescriptor descriptor)
    {
        var implementationType = descriptor.ImplementationType
                                 ?? descriptor.ImplementationInstance?.GetType();

        return implementationType is { IsGenericTypeDefinition: false }
               && typeof(IInitializable).IsAssignableFrom(implementationType);
    }
}