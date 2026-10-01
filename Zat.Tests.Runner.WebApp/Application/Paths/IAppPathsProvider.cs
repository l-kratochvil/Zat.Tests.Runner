namespace Zat.Tests.Runner.WebApp.Application.Paths;

using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// Where this application keeps everything it reads and writes.
/// </summary>
/// <remarks>
/// The place for the paths only this application uses; so far it uses only the shared ones.
/// </remarks>
public interface IAppPathsProvider : ISharedAppPathsProvider;