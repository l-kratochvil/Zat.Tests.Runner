namespace Zat.Tests.Runner.TuiApp.Application.Paths;

using Microsoft.Extensions.Options;

using Zat.Tests.Runner.Common.Net.Application;
using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The application paths, derived from the configured application data path.
/// </summary>
/// <param name="sharedAppOptions">The shared application options.</param>
internal sealed class AppPathsProvider(IOptions<ISharedAppOptions> sharedAppOptions)
    : AppPathsProviderBase<
        SharedAppDirectoryPaths,
        AppFilePaths,
        SharedAppFileExtensions,
        SharedAppFileNames,
        SharedAppDirectoryNames>(sharedAppOptions),
      IAppPathsProvider
{
    /// <inheritdoc/>
    protected override SharedAppDirectoryPaths CreateDirectories(SharedAppDirectoryPaths shared)
        => shared;

    /// <inheritdoc/>
    protected override AppFilePaths CreateFiles(SharedAppFilePaths shared)
        => new(
            shared,
            appState: Path.Combine(this.Directories.AppData, "app-state.json"));

    /// <inheritdoc/>
    protected override SharedAppFileExtensions CreateExtensions(SharedAppFileExtensions shared)
        => shared;

    /// <inheritdoc/>
    protected override SharedAppFileNames CreateFileNames(SharedAppFileNames shared)
        => shared;

    /// <inheritdoc/>
    protected override SharedAppDirectoryNames CreateDirectoryNames(SharedAppDirectoryNames shared)
        => shared;
}