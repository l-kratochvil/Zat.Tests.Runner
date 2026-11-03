namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// Failing test cases that attach a file from teardown.
/// </summary>
[TestFixture]
public class TearDownAttachmentFixture
{
    public const string UnnamedTestFailureReason = "UNNAMED FAILURE REASON";
    public const string NamedTestFailureReason = "NAMED FAILURE REASON";
    public const string AttachmentDescription = "TEARDOWN ATTACHMENT";
    public const string NamedTestName = "Named";

    /// <summary>
    /// Attaches a file named after the failed test case.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{testName}.txt");
        File.WriteAllText(filePath, testName);
        TestContext.AddTestAttachment(filePath, AttachmentDescription);
    }

    /// <summary>
    /// A failing test case.
    /// </summary>
    [Test]
    public void UnnamedTest()
    {
        Assert.Fail(UnnamedTestFailureReason);
    }

    /// <summary>
    /// A failing test case named by <see cref="TestCaseAttribute.TestName"/>, which NUnit nests under its method.
    /// </summary>
    [TestCase(TestName = NamedTestName)]
    public void NamedTest()
    {
        Assert.Fail(NamedTestFailureReason);
    }
}