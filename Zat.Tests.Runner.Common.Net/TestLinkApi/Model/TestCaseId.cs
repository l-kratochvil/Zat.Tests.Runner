namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// view of test case identifiers returned by the api call GetTestCaseIdByName
/// </summary>
/// <param name="Id">test case internal id</param>
/// <param name="Name"></param>
/// <param name="ParentId">that would be the id of the owning item in the nodes hierarchy table (i.e. the folder id)</param>
/// <param name="TcExternalId">the externally visible id without the prefix</param>
/// <param name="TsuiteName">name of the test suite that contains the test case</param>
public sealed record TestCaseId(
    int Id,
    string Name,
    int ParentId,
    int TcExternalId,
    string TsuiteName);