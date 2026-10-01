namespace Zat.Tests.Runner.Common.Net.Services;

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
        public IServiceCollection InitSharedServices<TAppOptions, TAppPathsProvider>(
            string appOptionsSectionName)
            where TAppOptions : AppOptionsBase
            where TAppPathsProvider : class, ISharedAppPathsProvider
            => services
                .InitAppOptions<TAppOptions>(appOptionsSectionName)
                .InitLogging()
                .AddScoped<WeakReferenceMessenger>()
                .AddSingleton<GlobalWeakReferenceMessanger>()
                .AddSingleton<TAppPathsProvider>()
                .AddSingleton<ISharedAppPathsProvider>(
                    static provider => provider.GetRequiredService<TAppPathsProvider>())
                .AddSingleton<ITestRunnerBridgeConnector, TestRunnerBridgeConnector>()
                .AddSingleton<ITestLink, TestLink>()
                .AddSingleton(_ => ITestLink.Config.Default)
                .AddSingleton<ITestRunnerEngine, TestRunnerEngine>()
                .AddSingleton<ITestResultHandler, TestLinkResultHandler>();

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
    }
}