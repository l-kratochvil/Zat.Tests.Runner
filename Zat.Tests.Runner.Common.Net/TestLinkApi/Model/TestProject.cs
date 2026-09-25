namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

using System.Collections.Generic;

/// <summary>
/// represent a test project object in testlink
/// </summary>
/// <param name="Active"></param>
/// <param name="Color"></param>
/// <param name="Id">internal id</param>
/// <param name="Name">project name</param>
/// <param name="Notes">notes</param>
/// <param name="OptionAutomation">true if automation is enabled</param>
/// <param name="OptionInventory">true of inventory is enabled</param>
/// <param name="OptionPriority">true if priority feature is enabled</param>
/// <param name="OptionReqs">true of requirements feature is enabled</param>
/// <param name="Prefix">string prefix for test cases</param>
/// <param name="TcCounter"></param>
public sealed record TestProject(
    bool Active,
    string Color,
    int Id,
    string Name,
    string Notes,
    bool OptionAutomation,
    bool OptionInventory,
    bool OptionPriority,
    bool OptionReqs,
    string Prefix,
    int TcCounter)
{
    public List<TestSuite> TestSuites { get; init; } = [];
}