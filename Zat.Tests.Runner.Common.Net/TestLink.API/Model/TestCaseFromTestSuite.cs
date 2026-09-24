namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

using System;

/// <summary>
/// test case as it is retrieved from testsuite
/// </summary>
/// <param name="Execution_type">manual or automatic</param>
/// <param name="External_id">the id that is displayed on the UI, sans the prefix</param>
/// <param name="Id">test case id</param>
/// <param name="Is_open">unknown purpose</param>
/// <param name="Layout">not clear what this represents</param>
/// <param name="Status">not clear in its meaning</param>
/// <param name="Tcversion_id">the internal id of this testcase version</param>
/// <param name="TestSuite_id">the id of the owning testsuite</param>
/// <param name="Version">the version of the test case, starts with 1</param>
public sealed record TestCaseFromTestSuite(
    bool Active,
    int Author_id,
    DateTime Creation_ts,
    string Details,
    int Execution_type,
    string External_id,
    int Id,
    int Importance,
    bool Is_open,
    string Layout,
    DateTime Modification_ts,
    string Name,
    int Node_order,
    string Node_table,
    int Node_type_id,
    int Parent_id,
    string Preconditions,
    int Status,
    string Summary,
    int Tcversion_id,
    int TestSuite_id,
    int Updater_id,
    int Version);