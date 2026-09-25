namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

using System;

/// <summary>
///  Represent the recorded outcome of a test case execution.
/// </summary>
/// <param name="BuildId">id of the build this was run against</param>
/// <param name="ExecutionTs">timestamp of execution</param>
/// <param name="ExecutionType">execution type, 1=manual, 2=automatic</param>
/// <param name="Id">internal id</param>
/// <param name="Notes">notes provided</param>
/// <param name="Status">status, p=pass, f=fail, b=blocked</param>
/// <param name="TcversionId">version id of test case</param>
/// <param name="TcversionNumber">external version number</param>
/// <param name="TesterId">id of tester</param>
/// <param name="TestplanId">id of testplan</param>
public sealed record ExecutionResult(
    int BuildId,
    DateTime ExecutionTs,
    int ExecutionType,
    int Id,
    string Notes,
    string Status,
    int TcversionId,
    int TcversionNumber,
    int TesterId,
    int TestplanId);