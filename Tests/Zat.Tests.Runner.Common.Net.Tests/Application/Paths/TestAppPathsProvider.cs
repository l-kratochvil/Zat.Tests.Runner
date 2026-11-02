namespace Zat.Tests.Runner.Common.Net.Tests.Application.Paths;

using Microsoft.Extensions.Options;

using Zat.Tests.Runner.Common.Net.Application;
using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// Application paths of no application in particular, for tests of the shared code.
/// </summary>
/// <param name="localAppDataPath">The local application data path.</param>
internal sealed class TestAppPathsProvider(string localAppDataPath)
    : AppPathsProviderBase<
        SharedAppDirectoryPaths,
        SharedAppFilePaths,
        SharedAppFileExtensions,
        SharedAppFileNames,
        SharedAppDirectoryNames>(localAppDataPath),
    ITestAppPathsProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAppPathsProvider"/> class the way the
    /// container of an application does.
    /// </summary>
    /// <param name="sharedAppOptions">The shared application options.</param>
    public TestAppPathsProvider(IOptions<ISharedAppOptions> sharedAppOptions)
        : this(sharedAppOptions.Value.LocalAppDataPath)
    {
    }

    protected override SharedAppDirectoryPaths CreateDirectories(SharedAppDirectoryPaths shared)
        => shared;

    protected override SharedAppFilePaths CreateFiles(SharedAppFilePaths shared)
        => shared;

    protected override SharedAppFileExtensions CreateExtensions(SharedAppFileExtensions shared)
        => shared;

    protected override SharedAppFileNames CreateFileNames(SharedAppFileNames shared)
        => shared;

    protected override SharedAppDirectoryNames CreateDirectoryNames(SharedAppDirectoryNames shared)
        => shared;
}