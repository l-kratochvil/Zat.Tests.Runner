namespace NUnitTestAssembly.Net461;

using NUnit.Framework;

/// <summary>
/// A single NUnit test suite used as build-time test data for the proxy tests.
/// </summary>
[TestFixture]
public class SampleTestSuite
{
    /// <summary>
    /// A single passing test case.
    /// </summary>
    [Test]
    public void SampleTestCase()
    {
        Assert.Pass();
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