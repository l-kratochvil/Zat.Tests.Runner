namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.TuiApp.Application.Paths;
using Zat.Tests.Runner.TuiApp.Stores;

internal static class InitStoresExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitStores()
            => hostBuilder.ConfigureServices(services => services
                .AddSingleton(static provider => AppUserSettingsStore.Create(
                    provider.GetRequiredService<IAppPathsProvider>()))
                .AddSingleton(static provider => AppStateStore.Create(
                    provider.GetRequiredService<IAppPathsProvider>()))
                .AddSingleton<TestRunConfigStore>());
    }
}