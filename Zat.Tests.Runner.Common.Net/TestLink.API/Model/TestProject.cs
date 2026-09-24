namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

using System.Collections.Generic;

/// <summary>
/// represent a test project object in testlink
/// </summary>
/// <param name="Active"></param>
/// <param name="Color"></param>
/// <param name="Id">internal id</param>
/// <param name="Name">project name</param>
/// <param name="Notes">notes</param>
/// <param name="Option_automation">true if automation is enabled</param>
/// <param name="Option_inventory">true of inventory is enabled</param>
/// <param name="Option_priority">true if priority feature is enabled</param>
/// <param name="Option_reqs">true of requirements feature is enabled</param>
/// <param name="Prefix">string prefix for test cases</param>
/// <param name="Tc_counter"></param>
public sealed record TestProject(
    bool Active,
    string Color,
    int Id,
    string Name,
    string Notes,
    bool Option_automation,
    bool Option_inventory,
    bool Option_priority,
    bool Option_reqs,
    string Prefix,
    int Tc_counter)
{
    public List<TestSuite> TestSuites { get; init; } = [];
}