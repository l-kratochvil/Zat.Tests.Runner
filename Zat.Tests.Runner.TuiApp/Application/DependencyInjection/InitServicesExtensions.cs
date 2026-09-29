namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zat.Tests.Runner.Common.Net.Logging;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.TuiApp.Application.Logging;
using Zat.Tests.Runner.TuiApp.Services;

internal static class InitServicesExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitServices(
            INUnitTestRunnerProxy nunitTestRunnerProxy)
            => hostBuilder.ConfigureServices(
                services => services
                    .InitLogging()
                    .AddSingleton<ITestLink, TestLink>()
                    .AddSingleton(_ => ITestLink.Config.Default)
                    .AddSingleton(nunitTestRunnerProxy)
                    .AddSingleton<ITestRunnerBridgeConnector, TestRunnerBridgeConnector>()
                    .AddSingleton<ITestRunnerEngine, TestRunnerEngine>()
                    .AddSingleton<ITestResultHandler, TestLinkResultHandler>()
                    .AddSingleton<TestLinkResultHandler.IContext, TestLinkResultHandlerContext>());
    }

    extension(IServiceCollection services)
    {
        private IServiceCollection InitLogging()
        {
            InitLoggingExtensions.InitLogging(services);
            return services.AddSingleton<IAppLoggerSink, FileLoggerSink>();
        }
    }
}