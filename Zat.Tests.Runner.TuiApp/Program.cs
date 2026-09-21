using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp;
using Zat.Tests.Runner.TuiApp.Application.DependencyInjection;

await using var nunitTestRunnerProxyConnector = await NUnitTestRunnerProxyConnector.ConnectAsync();

var host = Host
    .CreateDefaultBuilder()
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