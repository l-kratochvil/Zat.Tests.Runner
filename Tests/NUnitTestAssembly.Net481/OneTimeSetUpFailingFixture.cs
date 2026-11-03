namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// A fixture whose one-time setup fails before its test cases run.
/// </summary>
[TestFixture]
public class OneTimeSetUpFailingFixture
{
    public const string ErrorReason = "ONE TIME SETUP REASON";

    /// <summary>
    /// A failing one-time set-up.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        throw new Exception(ErrorReason);
    }

    /// <summary>
    /// A test case that should not run.
    /// </summary>
    [Test]
    public void FirstTest()
    {
    }

    /// <summary>
    /// A test case that should not run.
    /// </summary>
    [Test]
    public void SecondTest()
    {
    }
}