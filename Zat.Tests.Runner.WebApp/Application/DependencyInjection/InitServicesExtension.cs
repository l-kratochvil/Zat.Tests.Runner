namespace Zat.Tests.Runner.WebApp.Application.DependencyInjection;

using CommunityToolkit.Mvvm.Messaging;

using Fluxor;
using Fluxor.Persist.Middleware;
using Fluxor.Persist.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Features.TestResultReporting.Services;
using Zat.Tests.Runner.WebApp.Shared.JsInterop;
using Zat.Tests.Runner.WebApp.Shared.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

/// <summary>
/// Registration of services shared across features.
/// </summary>
public static class InitServicesExtension
{
    /// <param name="services">Service collection to extend.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the services that belong to no feature of their own.
        /// </summary>
        /// <remarks>
        /// The JS module services are scoped because <see cref="IJSRuntime"/> is scoped; see
        /// <see cref="JsModuleInteropFactory"/>.
        /// </remarks>
        /// <returns>The service collection, to allow chaining.</returns>
        public IServiceCollection InitServices()
            => services
                .InitSharedServices<
                    AppOptions,
                    AppPathsProvider,
                    IAppPathsProvider>(AppOptions.SectionName)
                .AddSingleton<IAppPathsProvider>(
                    static provider => provider.GetRequiredService<AppPathsProvider>())
                .InitFluxor()
                .InitNUnitTestRunner()
                .AddScoped<IJsModuleInteropFactory, JsModuleInteropFactory>()
                .AddScoped<ITestResultHandler, TestResultHandler>()
                .AddScoped<TestLinkResultHandler.IContext, TestLinkResultHandlerContext>()
                .AddScoped<WeakReferenceMessenger>()
                .AddSingleton<BrowserLogger>();

        private IServiceCollection InitNUnitTestRunner()
            => services
                .AddSingleton<NUnitTestRunnerStore>()
                .AddSingleton<INUnitTestRunnerStore>(
                    static provider => provider.GetRequiredService<NUnitTestRunnerStore>())
                .AddHostedService(static provider => provider.GetRequiredService<NUnitTestRunnerStore>());

        private IServiceCollection InitFluxor()
            => services
                .AddFluxor(
                    options => options
                        .ScanAssemblies(typeof(Program).Assembly)
                        .UsePersist(options =>
                        {
                            // Only what is listed here is remembered by the browser. The list is
                            // matched against the name of the feature, which Fluxor derives from
                            // the full name of the state.
                            options.UseInclusionApproach();
                            options.SetWhiteList([typeof(TestConfigurationState).FullName]);
                        }))
                .AddScoped<IStringStateStorage, LocalStringStateStorage>()
                .AddScoped<IStoreHandler, JsonStoreHandler>();
    }
}