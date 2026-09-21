using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Application.DependencyInjection;
using Zat.Tests.Runner.WebApp.Application.Logging;
using Zat.Tests.Runner.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// The NUnit runner lives in a process of its own, started here so that it is reachable for as long
// as the application is. Failing to reach it at all means the build did not put the server next to
// us or that it cannot run here, which is a fault of the installation rather than of the test run:
// starting up and pretending there are simply no tests would hide it.
await using var nunitTestRunnerProxyConnector = await NUnitTestRunnerProxyConnector.ConnectAsync();

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Logging.InitFileLogger();
builder.Services.AddSingleton(nunitTestRunnerProxyConnector.Proxy);
builder.Services.InitSharedServices();
builder.Services.InitFeatures();

var app = builder.Build();

// Setting up the filesystem is startup work, so it happens here and not in a constructor.
app.Services.InitInitializableServices(builder.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Lifecycle is developer detail, so it goes to the logging pipeline and not to the log panel.
// The category is spelled out because the generated Program class has no namespace and would not
// match the filters configured for the application.
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Zat.Tests.Runner.WebApp.Lifetime");
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() => logger.LogDebug("Application started."));
lifetime.ApplicationStopping.Register(() => logger.LogDebug("Application is shutting down (stopping)."));
lifetime.ApplicationStopped.Register(() => logger.LogDebug("Application is shutting down (stopped)."));

app.Run();