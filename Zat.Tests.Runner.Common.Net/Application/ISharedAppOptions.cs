namespace Zat.Tests.Runner.Common.Net.Application;

public interface ISharedAppOptions
{
    /// <summary>
    /// Gets the full path of the directory the application keeps its data in.
    /// </summary>
    public string LocalAppDataPath { get; }
}