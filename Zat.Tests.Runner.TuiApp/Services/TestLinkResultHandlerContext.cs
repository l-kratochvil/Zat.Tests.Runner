namespace Zat.Tests.Runner.TuiApp.Services;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp.Stores;

internal class TestLinkResultHandlerContext(
    TestRunConfigStore testRunConfigStore)
    : TestLinkResultHandler.IContext
{
    /// <inheritdoc/>
    public bool IsTestLinkReportingEnabled
        => testRunConfigStore.IsTestLinkReportingEnabled ?? false;

    /// <inheritdoc/>
    public bool IsDebuggingEnabled
        => testRunConfigStore.IsDebugModeEnabled ?? false;

    /// <inheritdoc/>
    public string? IdeVersion
        => testRunConfigStore.IdeVersion;

    /// <inheritdoc/>
    public string? IdeReleaseDate
        => testRunConfigStore.IdeReleaseDate;

    /// <inheritdoc/>
    public string? RuntimeVersion
        => testRunConfigStore.RuntimeVersion;

    /// <inheritdoc/>
    public string? RuntimeReleaseDate
        => testRunConfigStore.RuntimeReleaseDate;

    /// <inheritdoc/>
    public string? BetaVersion
        => testRunConfigStore.BetaVersion;
}