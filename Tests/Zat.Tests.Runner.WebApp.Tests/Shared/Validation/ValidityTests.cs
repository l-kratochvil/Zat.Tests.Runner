namespace Zat.Tests.Runner.WebApp.Tests.Shared.Validation;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.Validation;

[TestFixture]
public class ValidityTests
{
    private const string GivenField = "RuntimeVersion";
    private const string AnotherField = "IdeVersionText";

    [Test]
    public void HasErrors__WhenNothingWasFound__ThenShouldSayNothingIsWrong()
        => Assert.That(Validity.Valid.HasErrors, Is.False);

    [Test]
    public void HasErrors__WhenAnErrorWasFound__ThenShouldSaySo()
    {
        // Given:
        Validity unit = new([new Validity.Issue(GivenField, "Choose one.")]);

        // Then:
        Assert.That(unit.HasErrors, Is.True);
    }

    [Test]
    public void HasErrors__WhenOnlyWarningsWereFound__ThenShouldStillSayNothingIsWrong()
    {
        // Given:
        // A warning explains something to the tester without standing in their way.
        Validity unit = new(
            [new Validity.Issue(GivenField, "Nothing is installed there.", Validity.Severity.Warning)]);

        // Then:
        Assert.That(unit.HasErrors, Is.False);
    }

    [Test]
    public void For__WhenAskedAboutAField__ThenShouldKeepOnlyWhatConcernsIt()
    {
        // Given:
        Validity.Issue givenProblem = new(GivenField, "Choose one.");
        Validity unit = new([givenProblem, new Validity.Issue(AnotherField, "Write it as x.y.")]);

        // When:
        var narrowed = unit.For(GivenField);

        // Then:
        Assert.That(narrowed.Issues, Is.EqualTo(new[] { givenProblem }));
    }

    [Test]
    public void For__WhenTheFieldHasNoProblems__ThenShouldBeValid()
    {
        // Given:
        Validity unit = new([new Validity.Issue(AnotherField, "Write it as x.y.")]);

        // Then:
        Assert.That(unit.For(GivenField).HasErrors, Is.False);
    }
}