namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
///  represent a single test step in a test case
/// </summary>
/// <param name="Actions">string describing the actions</param>
/// <param name="Active">flag whether this step is active</param>
/// <param name="Execution_type">1=manual or 2=automated</param>
/// <param name="Expected_results">string desribing the expected result in this step</param>
/// <param name="Id">interenal primary key.</param>
/// <param name="Step_number">step number. Starts at 1</param>
public sealed record TestStep(
    string Actions,
    bool Active,
    int Execution_type,
    string Expected_results,
    int Id,
    int Step_number);