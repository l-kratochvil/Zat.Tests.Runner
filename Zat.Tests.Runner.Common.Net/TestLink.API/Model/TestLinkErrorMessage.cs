namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
/// package an the error message returned by the API
/// </summary>
/// <param name="Code">the testlink error code. See testlink API documentation.</param>
/// <param name="Message">the testlink error message returned</param>
public sealed record TestLinkErrorMessage(int Code, string Message);