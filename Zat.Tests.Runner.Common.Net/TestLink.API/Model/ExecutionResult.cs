namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

using System;

/// <summary>
///  Represent the recorded outcome of a test case execution.
/// </summary>
/// <param name="Build_id">id of the build this was run against</param>
/// <param name="Execution_ts">timestamp of execution</param>
/// <param name="Execution_type">execution type, 1=manual, 2=automatic</param>
/// <param name="Id">internal id</param>
/// <param name="Notes">notes provided</param>
/// <param name="Status">status, p=pass, f=fail, b=blocked</param>
/// <param name="Tcversion_id">version id of test case</param>
/// <param name="Tcversion_number">external version number</param>
/// <param name="Tester_id">id of tester</param>
/// <param name="Testplan_id">id of testplan</param>
public sealed record ExecutionResult(
    int Build_id,
    DateTime Execution_ts,
    int Execution_type,
    int Id,
    string Notes,
    string Status,
    int Tcversion_id,
    int Tcversion_number,
    int Tester_id,
    int Testplan_id);