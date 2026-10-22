namespace Zat.Tests.Runner.Common.Net.Application.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

using Zat.Tests.Runner.Common.Net.Extensions;

public static class InitLoggingExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Separates the log from diagnostics: <see cref="LogSources.App"/> is created from its own
        /// logger factory holding only providers of the log, so no filter configured for the main
        /// pipeline can leak it into the log file, console or debug output.
        /// </summary>
        /// <returns>The updated service collection.</returns>
        public IServiceCollection InitLogging()
            => services
                .AddKeyedSingleton<ILoggerFactory>(
                    IServicesCollectionExtensions.AppLoggingKey,
                    (provider, key) => new LoggerFactory(
                        provider.GetKeyedServices<ILoggerProvider>(key),
                        new LoggerFilterOptions { MinLevel = LogLevel.Trace }))
                .AddSingleton<ILogger<LogSources.App>>(
                    provider => new Logger<LogSources.App>(
                        provider.GetRequiredKeyedService<ILoggerFactory>(
                            IServicesCollectionExtensions.AppLoggingKey)))
                .AddSingleton<ILogger>(
                    provider => provider.GetRequiredService<ILogger<LogSources.Default>>());
    }

    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder InitLogging()
        {
            builder.Services.AddSingletonLoggerProvider<FileLoggerProvider>();

            // Binds Logging:File onto the options; the LogLevel subsection of that same section is
            // read by the filtering of the logging pipeline itself.
            LoggerProviderOptions.RegisterProviderOptions<
                FileLoggerOptions, FileLoggerProvider>(
                builder.Services);

            return builder;
        }
    }
}