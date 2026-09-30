namespace Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.TuiApp.Services;

internal static class InitServicesExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitServices(
            INUnitTestRunnerProxy nunitTestRunnerProxy)
            => hostBuilder.ConfigureServices(
                services => services
                    .AddLogging(builder => builder.InitLogging())
                    .InitSharedServices<AppOptions>(AppOptions.SectionName)
                    .AddSingleton(nunitTestRunnerProxy)
                    .AddSingleton<TestLinkResultHandler.IContext, TestLinkResultHandlerContext>());
    }
}