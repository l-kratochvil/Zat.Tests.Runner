namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// A single NUnit test suite used as build-time test data for the proxy tests.
/// </summary>
[TestFixture]
public class SampleTestSuite
{
    /// <summary>
    /// A passing test case.
    /// </summary>
    [Test]
    public void Pass()
    {
    }

    /// <summary>
    /// A failing test case.
    /// </summary>
    [Test]
    public void Fail()
    {
        Assert.Fail("FAILURE REASON");
    }

    /// <summary>
    /// An error test case.
    /// </summary>
    [Test]
    public void Error()
    {
        throw new Exception("ERROR REASON");
    }

    /// <summary>
    /// An ignored test case.
    /// </summary>
    [Test]
    public void Ignored()
    {
        Assert.Ignore("IGNORE REASON");
    }

    /// <summary>
    /// A passing test case with an attachment.
    /// </summary>
    [Test]
    public void WithAttachment()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "attachment.txt");
        File.WriteAllText(filePath, "ATTACHMENT");
        TestContext.AddTestAttachment(filePath, "ATTACHMENT DESCRIPTION");
    }
}