namespace NUnitTestAssembly.Net461.Nested;

using NUnit.Framework;

/// <summary>
/// A test fixture in a namespace nested one level deeper than <see cref="SampleTestSuite"/>, so that the proxy tests
/// cover test suites at different namespace depths.
/// </summary>
[TestFixture]
public class NestedTestSuite
{
    /// <summary>
    /// A single passing test case.
    /// </summary>
    [Test]
    public void NestedTestCase()
    {
        Assert.Pass();
    }
}