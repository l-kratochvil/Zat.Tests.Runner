namespace Zat.Tests.Runner.Common.Net.Application;

public abstract class AppOptionsBase : ISharedAppOptions
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// Environment variables are expanded, so the path may be spelled with them. Configuration is
    /// the only source: where the application writes is answered by the file the installation can
    /// edit rather than by a default compiled into it.
    /// </remarks>
    public string LocalAppDataPath
    {
        get;
        init => field = Environment.ExpandEnvironmentVariables(value);
    } = string.Empty;
}