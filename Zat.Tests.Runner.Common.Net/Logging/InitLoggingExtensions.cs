namespace Zat.Tests.Runner.Common.Net.Logging;

using Microsoft.Extensions.DependencyInjection;

public static class InitLoggingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection InitLogging()
            => services
                .AddSingleton<IAppLoggerFactory, AppLoggerFactory>()
                .AddSingleton(static provider =>
                    provider
                        .GetRequiredService<IAppLoggerFactory>()
                        .CreateLogger(LogSources.App))
                .AddSingleton<IAppLoggerHub, AppLoggerHub>();
    }
}