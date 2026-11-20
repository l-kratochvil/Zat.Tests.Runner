using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Application;
using Zat.Tests.Runner.Common.Net.Application.DependencyInjection;
using Zat.Tests.Runner.TuiApp;
using Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

AppConfiguration.UseDevelopmentWhenDebugConfig();

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        static config => config.AddEnvironmentVariables(AppConfiguration.EnvironmentVariablesPrefix));

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