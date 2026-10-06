namespace Zat.Tests.Runner.WebApp.Shared.Services;

using Fluxor;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

internal class TestLinkResultHandlerContext(
    IState<TestConfigurationState> testConfigurationState)
    : TestLinkResultHandler.IContext
{
    /// <inheritdoc/>
    public bool IsTestLinkReportingEnabled
        => testConfigurationState.Value.IsTestLinkReportEnabled;

    /// <inheritdoc/>
    public bool IsDebuggingEnabled
        => testConfigurationState.Value.IsDebugModeEnabled;

    /// <inheritdoc/>
    public string? IdeVersion
        => testConfigurationState.Value.IdeVersion?.ToString();

    /// <inheritdoc/>
    public string? IdeReleaseDate
        => testConfigurationState.Value.IdeReleaseDate;

    /// <inheritdoc/>
    public string? RuntimeVersion
        => testConfigurationState.Value.RuntimeVersion;

    /// <inheritdoc/>
    public string? RuntimeReleaseDate
        => testConfigurationState.Value.RuntimeReleaseDate;

    /// <inheritdoc/>
    public string? BetaVersion
        => testConfigurationState.Value.BetaVersion;
}