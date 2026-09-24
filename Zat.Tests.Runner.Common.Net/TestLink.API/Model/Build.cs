namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
///  Build information returned by Testlink
/// </summary>
/// <param name="Active">true if the build is active</param>
/// <param name="Id">build ID</param>
/// <param name="Is_open">true if the build is currently open</param>
/// <param name="Name">build name</param>
/// <param name="Notes">any build notes</param>
/// <param name="Testplan_id">the test plan the build is associated with</param>
public sealed record Build(
    bool Active,
    int Id,
    bool Is_open,
    string Name,
    string Notes,
    int Testplan_id);