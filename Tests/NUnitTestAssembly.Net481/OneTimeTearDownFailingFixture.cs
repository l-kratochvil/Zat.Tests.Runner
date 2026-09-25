namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// A test fixture whose <see cref="OneTimeTearDownAttribute"/> fails, used as build-time test data for the proxy tests.
/// </summary>
[TestFixture]
public class OneTimeTearDownFailingFixture
{
    /// <summary>
    /// A failing one-time tear-down.
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        throw new Exception("ONE TIME TEARDOWN REASON");
    }

    /// <summary>
    /// A passing test case.
    /// </summary>
    [Test]
    public void OneTimeTearDownFailing_First()
    {
    }

    /// <summary>
    /// A passing test case.
    /// </summary>
    [Test]
    public void OneTimeTearDownFailing_Second()
    {
    }
}