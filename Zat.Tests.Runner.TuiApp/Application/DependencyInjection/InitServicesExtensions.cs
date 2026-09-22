namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;

internal static class InitServicesExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitServices(
            INUnitTestRunnerProxy nunitTestRunnerProxy)
            => hostBuilder.ConfigureServices(
                services => services
                    .AddSingleton<ITestLinkApiClient, TestLinkApiClient>()
                    .AddSingleton(_ => ITestLinkApiClient.Config.Default)
                    .AddSingleton(nunitTestRunnerProxy)
                    .AddSingleton<ITestRunnerBridgeConnector, TestRunnerBridgeConnector>()
                    .AddSingleton<ITestResultHandler, TestLinkResultHandler>()
                    .AddSingleton<ITestRunnerEngine, TestRunnerEngine>());
    }
}