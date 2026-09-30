namespace Zat.Tests.Runner.Common.Net.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class IServicesCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSingletonLoggerProvider<TLoggerProvider>()
            where TLoggerProvider : class, ILoggerProvider
        {
            services
                .AddSingleton<TLoggerProvider>()
                .AddSingleton<ILoggerProvider>(
                    provider => provider.GetRequiredService<TLoggerProvider>());

            return services;
        }

        public IServiceCollection AddScopedLoggerProvider<TLoggerProvider>()
            where TLoggerProvider : class, ILoggerProvider
        {
            services
                .AddScoped<TLoggerProvider>()
                .AddScoped<ILoggerProvider>(
                    provider => provider.GetRequiredService<TLoggerProvider>());

            return services;
        }
    }
}