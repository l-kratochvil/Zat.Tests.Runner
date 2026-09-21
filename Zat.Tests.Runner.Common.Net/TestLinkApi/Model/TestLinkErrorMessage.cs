namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// package an the error message returned by the API
/// </summary>
public class TestLinkErrorMessage
{
    /// <summary>
    /// the testlink error code. See testlink API documentation.
    /// </summary>
    public int code;

    /// <summary>
    /// the testlink error message returned
    /// </summary>
    public string message;
}