namespace Zat.Tests.Runner.WebApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Zat.Tests.Runner.Common.Net.Extensions;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;
using Zat.Tests.Runner.WebApp.Features.AppSettings.Services;
using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Features.TestDiscovery.Services;
using Zat.Tests.Runner.WebApp.Features.TestExecution.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

/// <summary>
/// Feature-by-feature registration of application services.
/// </summary>
public static class InitFeaturesExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection InitFeatures()
            => services
                .InitAppSettings()
                .InitTestDiscovery()
                .InitTestConfiguration()
                .InitTestExecution()
                .InitAppLogging();

        private IServiceCollection InitTestDiscovery()
            => services
                .AddScoped<TestDiscoveryStore>()
                .AddScoped<ITestDiscoveryStore>(
                    provider => provider.GetRequiredService<TestDiscoveryStore>());

        /// <remarks>
        /// The settings describe one machine, so one <see cref="AppSettingsStore"/> is shared across
        /// circuits and started before the first browser response.
        /// </remarks>
        private IServiceCollection InitAppSettings()
            => services
                .AddSingleton<AppSettingsStore>()
                .AddSingleton<IAppSettingsStore>(
                    static provider => provider.GetRequiredService<AppSettingsStore>())
                .AddHostedService(static provider => provider.GetRequiredService<AppSettingsStore>());

        private IServiceCollection InitTestConfiguration()
            => services
                .AddScoped<TestConfigurationViewModel>();

        private IServiceCollection InitTestExecution()
            => services
                .AddScoped<TestExecutionViewModel>();

        private IServiceCollection InitAppLogging()
            => services
                .AddSingletonLoggerProvider<AppLoggerProvider>();
    }
}