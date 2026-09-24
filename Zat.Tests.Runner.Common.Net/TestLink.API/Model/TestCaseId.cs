namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
/// view of test case identifiers returned by the api call GetTestCaseIdByName
/// </summary>
/// <param name="Id">test case internal id</param>
/// <param name="Name"></param>
/// <param name="Parent_id">that would be the id of the owning item in the nodes hierarchy table (i.e. the folder id)</param>
/// <param name="Tc_external_id">the externally visible id without the prefix</param>
/// <param name="Tsuite_name">name of the test suite that contains the test case</param>
public sealed record TestCaseId(
    int Id,
    string Name,
    int Parent_id,
    int Tc_external_id,
    string Tsuite_name);