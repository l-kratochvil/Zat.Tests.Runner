namespace Zat.Tests.Runner.WebApp.Application.Paths;

using Microsoft.Extensions.Options;

using Zat.Tests.Runner.Common.Net.Application;
using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The application paths, derived from the configured application data path.
/// </summary>
/// <param name="sharedAppOptions">The shared application options.</param>
public sealed class AppPathsProvider(IOptions<ISharedAppOptions> sharedAppOptions)
    : AppPathsProviderBase<
        SharedAppDirectoryPaths,
        SharedAppFilePaths,
        SharedAppFileExtensions,
        SharedAppFileNames,
        SharedAppDirectoryNames>(sharedAppOptions),
      IAppPathsProvider
{
    /// <inheritdoc/>
    protected override SharedAppDirectoryPaths CreateDirectories(SharedAppDirectoryPaths shared)
        => shared;

    /// <inheritdoc/>
    protected override SharedAppFilePaths CreateFiles(SharedAppFilePaths shared)
        => shared;

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