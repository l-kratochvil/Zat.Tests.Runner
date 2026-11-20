namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// Additional Info is provided in some cases when objects are created.
/// <see cref="GeneralResult"/>
/// </summary>
/// <param name="ExternalId">external id if used</param>
/// <param name="HasDuplicate">true if a duplicate exists</param>
/// <param name="Id">internal id</param>
/// <param name="Msg">extra message, e.g."Created new version 2"</param>
/// <param name="NewName">new namee given</param>
/// <param name="StatusOk">true means good</param>
/// <param name="VersionNumber">version number in test cases</param>
public sealed record AdditionalInfo(
    int ExternalId,
    bool? HasDuplicate,
    int Id,
    string Msg,
    string NewName,
    bool StatusOk,
    int VersionNumber);