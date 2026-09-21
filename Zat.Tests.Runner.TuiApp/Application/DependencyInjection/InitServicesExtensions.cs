namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.TuiApp.Common;
using Zat.Tests.Runner.TuiApp.TestLinkApi;

internal static class InitServicesExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitServices(
            INUnitTestRunnerProxy nunitTestRunnerProxy)
            => hostBuilder.ConfigureServices(
                services => services
                    .AddSingleton(AppSystemConfig.CreateDefault())
                    .AddSingleton<ITestLinkApiClient, TestLinkApiClient>()
                    .AddSingleton(nunitTestRunnerProxy)
                    .AddSingleton<ITestRunnerBridgeConnector, TestRunnerBridgeConnector>()
                    .AddSingleton<ITestRunnerEngine, TestRunnerEngine>());
    }
}