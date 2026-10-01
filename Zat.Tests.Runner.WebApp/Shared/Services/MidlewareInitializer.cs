namespace Zat.Tests.Runner.WebApp.Shared.Services;

using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;

/// <summary>
/// Represents a class responsible for initializing middleware components.
/// These components are used by no other service so to be created it's necessary to request theme somewhere.
/// </summary>
/// <param name="appLoggerMidleware">The <see cref="AppLoggerMidleware"/> midleware.</param>
public class MidlewareInitializer(
#pragma warning disable CS9113 // Parameter is unread.
    AppLoggerMidleware appLoggerMidleware);
#pragma warning restore CS9113 // Parameter is unread.