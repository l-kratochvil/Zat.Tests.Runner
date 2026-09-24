namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
/// view of test case identifiers returned by the api call GetTestCaseIdByName
/// </summary>
/// <param name="Id">test case internal id</param>
/// <param name="Name">Gets the name of the test case,</param>
/// <param name="ParentId">that would be the id of the owning item in the nodes hierarchy table (i.e. the folder id)</param>
/// <param name="ExternalId">the externally visible id without the prefix</param>
public sealed record TestCase(
    int Id,
    string Name,
    int ExternalId,
    int TestSuiteId);