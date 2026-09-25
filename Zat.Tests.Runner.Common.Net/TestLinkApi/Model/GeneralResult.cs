namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
///  returned when creating new TestProjects, TestCases, projects etc
/// </summary>
/// <param name="AdditionalInfo">Any potential additional information</param>
/// <param name="Id">id of an object involved in the operation. e.g. test case id</param>
/// <param name="Message">the message returned by Testlink</param>
/// <param name="Operation">the name of the operation carried out</param>
/// <param name="Status">a status. True means good</param>
public sealed record GeneralResult(
    AdditionalInfo? AdditionalInfo,
    int Id,
    string Message,
    string Operation,
    bool Status)
{
    /// <summary>
    ///  used by the Exporter class
    /// </summary>
    /// <param name="message"></param>
    /// <param name="status"></param>
    public GeneralResult(string message, bool status)
        : this(AdditionalInfo: null, Id: 0, Message: message, Operation: string.Empty, Status: status)
    {
    }

    /// <summary>
    ///  conbstructor used to represent an empty response
    /// </summary>
    public GeneralResult()
        : this(message: "no response from server", status: false)
    {
    }
}