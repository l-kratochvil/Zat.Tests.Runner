namespace Zat.Tests.Runner.TuiApp.Application.Paths;

using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// Where this application keeps everything it reads and writes.
/// </summary>
/// <remarks>
/// Adds the paths only this application uses to the shared ones.
/// </remarks>
internal interface IAppPathsProvider : ISharedAppPathsProvider
{
    /// <inheritdoc cref="ISharedAppPathsProvider.Files"/>
    new AppFilePaths Files { get; }
}