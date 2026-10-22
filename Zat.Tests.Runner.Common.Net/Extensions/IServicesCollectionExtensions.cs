namespace Zat.Tests.Runner.Common.Net.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class IServicesCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers a provider of diagnostics: it receives every category except <see cref="LogSources.App"/>.
        /// </summary>
        /// <typeparam name="TLoggerProvider">Logger provider type.</typeparam>
        /// <returns>The updated service collection.</returns>
        public IServiceCollection AddSingletonLoggerProvider<TLoggerProvider>()
            where TLoggerProvider : class, ILoggerProvider
        {
            services
                .AddSingleton<TLoggerProvider>()
                .AddSingleton<ILoggerProvider>(
                    provider => provider.GetRequiredService<TLoggerProvider>());

            return services;
        }

        /// <summary>
        /// Registers a provider of the log: it receives only <see cref="LogSources.App"/>.
        /// </summary>
        /// <typeparam name="TLoggerProvider">Logger provider type.</typeparam>
        /// <returns>The updated service collection.</returns>
        public IServiceCollection AddSingletonAppLoggerProvider<TLoggerProvider>()
            where TLoggerProvider : class, ILoggerProvider
        {
            services
                .AddSingleton<TLoggerProvider>()
                .AddKeyedSingleton<ILoggerProvider>(
                    AppLoggingKey,
                    (provider, _) => provider.GetRequiredService<TLoggerProvider>());

            return services;
        }
    }

    /// <summary>
    /// Key under which the providers of the log (<see cref="LogSources.App"/>) and its logger factory are registered.
    /// </summary>
    public const string AppLoggingKey = "Zat.Tests.Runner.AppLogging";
}
