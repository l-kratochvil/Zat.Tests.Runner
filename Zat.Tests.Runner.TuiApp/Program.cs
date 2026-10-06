using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Application.DependencyInjection;
using Zat.Tests.Runner.TuiApp;
using Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

var hostBuilder = Host.CreateDefaultBuilder();

#if DEBUG
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")))
{
    hostBuilder.UseEnvironment(Environments.Development);
}
#endif

IServiceCollection registeredServices = new ServiceCollection();

var host = hostBuilder
    .InitServices()
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

host.Services.InitInitializableServices(registeredServices, ServiceLifetime.Singleton);

await App.RunAsync(host);