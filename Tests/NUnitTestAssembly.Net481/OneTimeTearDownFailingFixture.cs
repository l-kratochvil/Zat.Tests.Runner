namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

using NUnitTestAssembly.Common;

/// <summary>
/// A fixture whose one-time teardown fails after its test cases run.
/// </summary>
[TestFixture]
[Category(TestCaseFixtureBase.RunnerTestFixtureCategory)]
public class OneTimeTearDownFailingFixture
{
    public const string ErrorReason = "ONE TIME TEARDOWN REASON";

    /// <summary>
    /// A failing one-time tear-down.
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        throw new Exception(ErrorReason);
    }


    /// <summary>
    /// A test case that should pass.
    /// </summary>
    [Test]
    public void FirstTest()
    {
    }

    /// <summary>
    /// A test case that should pass.
    /// </summary>
    [Test]
    public void SecondTest()
    {
    }
}