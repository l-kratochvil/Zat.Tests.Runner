namespace Zat.Tests.Runner.WebApp.Shared.Services;

using Zat.Tests.Runner.Common.Net.Services;

internal class TestLinkResultHandlerContext : TestLinkResultHandler.IContext
{
    /// <inheritdoc/>
    public bool IsTestLinkReportingEnabled
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public bool IsDebuggingEnabled
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public string? IdeVersion
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public string? IdeReleaseDate
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public string? RuntimeVersion
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public string? RuntimeReleaseDate
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public string? BetaVersion
        => throw new NotImplementedException();
}