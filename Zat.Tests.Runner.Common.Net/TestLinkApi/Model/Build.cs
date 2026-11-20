namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
///  Build information returned by Testlink
/// </summary>
/// <param name="Active">true if the build is active</param>
/// <param name="Id">build ID</param>
/// <param name="IsOpen">true if the build is currently open</param>
/// <param name="Name">build name</param>
/// <param name="Notes">any build notes</param>
/// <param name="TestplanId">the test plan the build is associated with</param>
public sealed record Build(
    bool Active,
    int Id,
    bool IsOpen,
    string Name,
    string Notes,
    int TestplanId);