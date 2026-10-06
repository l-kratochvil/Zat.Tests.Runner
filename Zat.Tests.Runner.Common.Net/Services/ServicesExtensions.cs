namespace Zat.Tests.Runner.Common.Net.Services;

using System.Diagnostics;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Zat.Tests.Runner.Common.Net.Application;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Application.Paths;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Initializes the shared services for the application.
        /// </summary>
        /// <typeparam name="TAppOptions">App options service type.</typeparam>
        /// <typeparam name="TAppPathsProvider">App paths provider service type.</typeparam>
        /// <typeparam name="TAppPathsProviderInterface">App paths provider interface type under which the service will be registered.</typeparam>
        /// <param name="appOptionsSectionName">The name of the configuration section for the app options.</param>
        /// <returns>The updated service collection.</returns>
        public IServiceCollection InitSharedServices<
            TAppOptions,
            TAppPathsProvider,
            TAppPathsProviderInterface>(
            string appOptionsSectionName)
            where TAppOptions : AppOptionsBase
            where TAppPathsProvider : class, ISharedAppPathsProvider, TAppPathsProviderInterface
            where TAppPathsProviderInterface : class
            => services
                .InitAppOptions<TAppOptions>(appOptionsSectionName)
                .InitAppPathsProvider<TAppPathsProvider, TAppPathsProviderInterface>()
                .InitLogging()
                .AddScoped<WeakReferenceMessenger>()
                .AddSingleton<GlobalWeakReferenceMessanger>()
                .AddSingleton<ITestRunnerBridgeConnector, TestRunnerBridgeConnector>()
                .AddSingleton<INUnitTestRunnerProxyConnector>(
                    new NUnitTestRunnerProxyConnector(launchDebugger: !Debugger.IsAttached))
                .AddSingleton<ITestLink, TestLink>()
                .AddSingleton(_ => ITestLink.Config.Default)
                .AddSingleton<ITestRunnerEngine, TestRunnerEngine>()
                .AddSingleton(TimeProvider.System)
                .AddSingleton<IDirectoryWatcher, FileSystemDirectoryWatcher>()
                .AddSingleton<ITestTreeStore, TestTreeStore>();

        private IServiceCollection InitAppOptions<TOptions>(
            string sectionName)
            where TOptions : AppOptionsBase
        {
            services
                .AddOptions<TOptions>()
                .BindConfiguration(
                    sectionName,
                    static binderOptions => binderOptions.ErrorOnUnknownConfiguration = true)
                .Validate(
                    static options => !string.IsNullOrWhiteSpace(options.LocalAppDataPath)
                                      && Path.IsPathFullyQualified(options.LocalAppDataPath),
                    $"'{sectionName}:{nameof(AppOptionsBase.LocalAppDataPath)}' has to be an absolute path.")
                .ValidateOnStart();

            return services.AddSingleton<IOptions<ISharedAppOptions>>(
                sp => sp.GetRequiredService<IOptions<TOptions>>());
        }

        private IServiceCollection InitAppPathsProvider<TAppPathsProvider, TAppPathsProviderInterface>()
            where TAppPathsProvider : class, ISharedAppPathsProvider, TAppPathsProviderInterface
            where TAppPathsProviderInterface : class
            => services
                .AddSingleton<TAppPathsProvider>()
                .AddSingleton<ISharedAppPathsProvider>(
                    static provider => provider.GetRequiredService<TAppPathsProvider>())
                .AddSingleton<TAppPathsProviderInterface>(
                    static provider => provider.GetRequiredService<TAppPathsProvider>());
    }
}