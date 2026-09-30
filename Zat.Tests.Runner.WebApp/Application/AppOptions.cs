namespace Zat.Tests.Runner.WebApp.Application;

using Zat.Tests.Runner.Common.Net.Application;

/// <summary>
/// Options configuring where the application keeps its data, bound from <c>App</c>.
/// </summary>
/// <remarks>
/// The root is configured; everything below it is derived, see <see cref="IAppPathsProvider"/>.
/// </remarks>
public sealed class AppOptions : AppOptionsBase
{
    /// <summary>
    /// Name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = "App";
}
