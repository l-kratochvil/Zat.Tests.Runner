namespace Zat.Tests.Runner.WebApp.Tests.Application.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Net.Application.Logging;

/// <summary>
/// Registers logging the way the application start-up does, so services asking for a plain
/// <see cref="ILogger"/> resolve in tests too.
/// </summary>
internal static class TestLoggingExtensions
{
    public static IServiceCollection InitLoggingAsApp(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configure = null)
        => services
            .AddLogging(builder => configure?.Invoke(builder))
            .InitLogging();
}