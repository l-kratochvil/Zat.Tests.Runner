using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Application.DependencyInjection;
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

IServiceCollection registeredServices = new ServiceCollection();

var host = hostBuilder
    .InitServices(nunitTestRunnerProxyConnector.Proxy)
    .InitScreens()
    .InitStores()
    .ConfigureServices(services => registeredServices = services)
    .UseDefaultServiceProvider(
        (_, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        })
    .Build();

host.Services.InitInitializableServices(registeredServices);

await App.RunAsync(host);