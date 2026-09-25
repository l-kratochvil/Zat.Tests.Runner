namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// An ignored test fixture, used as build-time test data for the proxy tests.
/// </summary>
[TestFixture]
[Ignore("FIXTURE IGNORE REASON")]
public class IgnoredFixture
{
    /// <summary>
    /// A test case that would pass.
    /// </summary>
    [Test]
    public void IgnoredFixture_First()
    {
    }

    /// <summary>
    /// A test case that would pass.
    /// </summary>
    [Test]
    public void IgnoredFixture_Second()
    {
    }
}