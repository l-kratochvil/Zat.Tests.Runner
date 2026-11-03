namespace NUnitTestAssembly.Common;

using NUnit.Framework;

public abstract class TestCaseFixtureBase
{
    public const string FailureReason = "FAILURE REASON";
    public const string ErrorReason = "ERROR REASON";
    public const string IgnoreReason = "IGNORE REASON";
    public const string AttachmentDescription = "ATTACHMENT DESCRIPTION";
    public const string AttachmentFileName = "attachment.txt";

    /// <summary>
    /// A passing test case.
    /// </summary>
    [Test]
    public void PassingTest()
    {
    }

    /// <summary>
    /// A failing test case.
    /// </summary>
    [Test]
    public void FailingTest()
    {
        Assert.Fail(FailureReason);
    }

    /// <summary>
    /// A test case that throws an exception.
    /// </summary>
    [Test]
    public void ThrowingTest()
    {
        throw new Exception(ErrorReason);
    }

    /// <summary>
    /// An ignored test case.
    /// </summary>
    [Test]
    public void IgnoredTest()
    {
        Assert.Ignore(IgnoreReason);
    }

    /// <summary>
    /// A passing test case with an attachment.
    /// </summary>
    [Test]
    public void AttachingTest()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, AttachmentFileName);
        File.WriteAllText(filePath, "ATTACHMENT");
        TestContext.AddTestAttachment(filePath, AttachmentDescription);
    }
}