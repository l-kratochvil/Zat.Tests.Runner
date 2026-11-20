namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// view of test case identifiers returned by the api call GetTestCaseIdByName
/// </summary>
/// <param name="Id">test case internal id</param>
/// <param name="Name">Gets the name of the test case,</param>
/// <param name="ExternalId">the externally visible id without the prefix</param>
/// <param name="TestSuiteId">Id of test suite owning the test case</param>
public sealed record TestCase(
    int Id,
    string Name,
    int ExternalId,
    int TestSuiteId);