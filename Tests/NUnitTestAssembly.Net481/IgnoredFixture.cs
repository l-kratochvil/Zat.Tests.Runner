namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

using NUnitTestAssembly.Common;

/// <summary>
/// A fixture ignored as a whole.
/// </summary>
[TestFixture]
[Category(TestCaseFixtureBase.RunnerTestFixtureCategory)]
[Ignore(IgnoreReason)]
public class IgnoredFixture
{
    public const string IgnoreReason = "FIXTURE IGNORE REASON";

    /// <summary>
    /// A test case that should be ignored.
    /// </summary>
    [Test]
    public void FirstTest()
    {
    }

    /// <summary>
    /// A test case that should be ignored.
    /// </summary>
    [Test]
    public void SecondTest()
    {
    }
}