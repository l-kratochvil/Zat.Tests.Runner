namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

using System.Collections.Generic;

/// <summary>
///  represent a folder in the test specification tree
/// </summary>
/// <param name="Id">internal primary key</param>
/// <param name="Name">name of test suite</param>
/// <param name="Details">details of test suite</param>
/// <param name="NodeOrder">sequence id for ordering folders in tree</param>
/// <param name="NodeTypeId">internal value</param>
/// <param name="ParentId">foreign key to parent</param>
public sealed record TestSuite(
    int Id,
    string Name,
    string Details,
    int NodeOrder,
    int NodeTypeId,
    int ParentId)
{
    public List<TestCaseFromTestSuite> TestCases { get; init; } = [];

    public List<TestSuite> TestSuites { get; init; } = [];

    public void AddTestSuite(TestSuite testSuite)
    {
        this.TestSuites.Add(testSuite);
    }

    // Add a test case to the suite
    public void AddTestCase(TestCaseFromTestSuite testCase)
    {
        this.TestCases.Add(testCase);
    }
}