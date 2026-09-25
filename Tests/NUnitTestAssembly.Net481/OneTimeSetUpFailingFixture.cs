namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// A test fixture whose <see cref="OneTimeSetUpAttribute"/> fails, used as build-time test data for the proxy tests.
/// </summary>
[TestFixture]
public class OneTimeSetUpFailingFixture
{
    /// <summary>
    /// A failing one-time set-up.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        throw new Exception("ONE TIME SETUP REASON");
    }

    /// <summary>
    /// A test case that would pass.
    /// </summary>
    [Test]
    public void OneTimeSetUpFailing_First()
    {
    }

    /// <summary>
    /// A test case that would pass.
    /// </summary>
    [Test]
    public void OneTimeSetUpFailing_Second()
    {
    }
}