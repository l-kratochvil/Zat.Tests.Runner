namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
/// represent a test plan
/// </summary>
/// <param name="active">True if the plan is currently active</param>
/// <param name="id">primary key</param>
/// <param name="is_public"></param>
/// <param name="name"></param>
/// <param name="notes"></param>
/// <param name="open"></param>
/// <param name="testproject_id">foreign key to test project</param>
public sealed record TestPlan(
    bool active,
    int id,
    bool is_public,
    string name,
    string notes,
    bool open,
    int testproject_id);