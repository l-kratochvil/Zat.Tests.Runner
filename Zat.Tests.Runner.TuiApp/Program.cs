using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp;
using Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

await using var nunitTestRunnerProxyConnector = await NUnitTestRunnerProxyConnector.ConnectAsync();

var hostBuilder = Host.CreateDefaultBuilder();

#if DEBUG
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")))
{
    hostBuilder.UseEnvironment(Environments.Development);
}
#endif

var host = hostBuilder
    .InitServices(nunitTestRunnerProxyConnector.Proxy)
    .InitScreens()
    .InitStores()
    .UseDefaultServiceProvider(
        (_, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        })
    .Build();

await App.RunAsync(host);