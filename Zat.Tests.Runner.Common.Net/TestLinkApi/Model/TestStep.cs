namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
///  represent a single test step in a test case
/// </summary>
/// <param name="Actions">string describing the actions</param>
/// <param name="Active">flag whether this step is active</param>
/// <param name="ExecutionType">1=manual or 2=automated</param>
/// <param name="ExpectedResults">string desribing the expected result in this step</param>
/// <param name="Id">interenal primary key.</param>
/// <param name="StepNumber">step number. Starts at 1</param>
public sealed record TestStep(
    string Actions,
    bool Active,
    int ExecutionType,
    string ExpectedResults,
    int Id,
    int StepNumber);