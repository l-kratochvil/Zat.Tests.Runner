namespace Zat.Tests.Runner.Common.Net.Application.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

using Zat.Tests.Runner.Common.Net.Extensions;

public static class InitLoggingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection InitLogging()
            => services
                .AddSingleton<ILogger>(
                    provider => provider.GetRequiredService<ILogger<LogSources.App>>());
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