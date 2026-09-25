namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// represent a test plan
/// </summary>
/// <param name="Active">True if the plan is currently active</param>
/// <param name="Id">primary key</param>
/// <param name="IsPublic"></param>
/// <param name="Name"></param>
/// <param name="Notes"></param>
/// <param name="Open"></param>
/// <param name="TestprojectId">foreign key to test project</param>
public sealed record TestPlan(
    bool Active,
    int Id,
    bool IsPublic,
    string Name,
    string Notes,
    bool Open,
    int TestprojectId);