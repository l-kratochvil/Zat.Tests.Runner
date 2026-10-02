namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.TuiApp.Application.Paths;
using Zat.Tests.Runner.TuiApp.Services;

internal static class InitServicesExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitServices(
            INUnitTestRunnerProxy nunitTestRunnerProxy)
            => hostBuilder.ConfigureServices(
                services => services
                    .AddLogging(builder => RemoveConsoleLogger(builder).InitLogging())
                    .InitSharedServices<
                        AppOptions,
                        AppPathsProvider,
                        IAppPathsProvider>(AppOptions.SectionName)
                    .AddSingleton<IAppPathsProvider>(
                        static provider => provider.GetRequiredService<AppPathsProvider>())
                    .AddSingleton(nunitTestRunnerProxy)
                    .AddSingleton<TestLinkResultHandler.IContext, TestLinkResultHandlerContext>()
                    .AddSingleton<ITestResultHandler, TestLinkResultHandler>());
    }

    /// <summary>
    /// Removes the <see cref="ConsoleLoggerProvider"/> registered by the default host: the TUI owns the
    /// console and log lines written into it break its live rendering.
    /// </summary>
    private static ILoggingBuilder RemoveConsoleLogger(ILoggingBuilder builder)
    {
        var consoleLoggerDescriptors = builder.Services
            .Where(x => x.ServiceType == typeof(ILoggerProvider) &&
                        x.ImplementationType == typeof(ConsoleLoggerProvider))
            .ToArray();

        foreach (var descriptor in consoleLoggerDescriptors)
        {
            builder.Services.Remove(descriptor);
        }

        return builder;
    }
}