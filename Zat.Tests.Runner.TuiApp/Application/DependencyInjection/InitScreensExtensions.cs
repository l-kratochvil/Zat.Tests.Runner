namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.TuiApp.Screens;

internal static class InitScreensExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitScreens()
            => hostBuilder.ConfigureServices(services => services
                .AddSingleton<HomeScreen>()
                .AddSingleton<EmptyScreen>()
                .AddSingleton<ExitScreen>()
                .AddSingleton<SettingsScreen>()
                .AddSingleton<IdeVersionPromptScreen>()
                .AddSingleton<RuntimeVersionPromptScreen>()
                .AddSingleton<TestSuitesSelectionScreen>()
                .AddSingleton<TestCasesSelectionScreen>()
                .AddSingleton<HwAssemblyTypesSelectionScreen>()
                .AddSingleton<EnableTestLinkReportingPromptScreen>()
                .AddSingleton<IdeReleaseDatePromptScreen>()
                .AddSingleton<RuntimeReleaseDatePromptScreen>()
                .AddSingleton<EnableDebugModePromptScreen>()
                .AddSingleton<IsBetaVersionPromptScreen>()
                .AddSingleton<BetaVersionPromptScreen>()
                .AddSingleton(static provider => new Lazy<HomeScreen>(provider.GetRequiredService<HomeScreen>))
                .AddSingleton(static provider => new Lazy<ExitScreen>(provider.GetRequiredService<ExitScreen>))
                .AddSingleton(static provider => new Lazy<SettingsScreen>(provider.GetRequiredService<SettingsScreen>))
                .AddTransient<RunTestScreen>());
    }
}