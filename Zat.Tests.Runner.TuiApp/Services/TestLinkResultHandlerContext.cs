namespace Zat.Tests.Runner.TuiApp.Services;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp.Stores;

internal class TestLinkResultHandlerContext(
    TestConfigStore testConfigStore)
    : TestLinkResultHandler.IContext
{
    /// <inheritdoc/>
    public bool IsTestLinkReportingEnabled
        => testConfigStore.IsTestLinkReportingEnabled ?? false;

    /// <inheritdoc/>
    public string? IdeVersion
        => testConfigStore.IdeVersion;

    /// <inheritdoc/>
    public string? IdeReleaseDate
        => testConfigStore.IdeReleaseDate;

    /// <inheritdoc/>
    public string? RuntimeVersion
        => testConfigStore.RuntimeVersion;

    /// <inheritdoc/>
    public string? RuntimeReleaseDate
        => testConfigStore.RuntimeReleaseDate;
}