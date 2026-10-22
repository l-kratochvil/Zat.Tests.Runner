namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// Failing test cases that attach a file in <see cref="TearDown"/>, as the test assemblies do with failure screenshots.
/// </summary>
[TestFixture]
public class TearDownAttachmentFixture
{
    /// <summary>
    /// A failing test case.
    /// </summary>
    [Test]
    public void Plain()
    {
        Assert.Fail("PLAIN FAILURE REASON");
    }

    /// <summary>
    /// A failing test case named by <see cref="TestCaseAttribute.TestName"/>, which NUnit nests under its method.
    /// </summary>
    [Test]
    [TestCase(TestName = "Named")]
    public void NamedMethod()
    {
        Assert.Fail("NAMED FAILURE REASON");
    }

    /// <summary>
    /// Attaches a file named after the failed test case.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{testName}.txt");
        File.WriteAllText(filePath, testName);
        TestContext.AddTestAttachment(filePath, "TEARDOWN ATTACHMENT");
    }
}