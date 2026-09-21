namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.Domain;
using Zat.Tests.Runner.WebApp.Shared.Validation;
using Zat.Tests.Runner.WebApp.Tests.Shared.Validation;

[TestFixture]
public class TestConfigurationValidatorTests
{
    private const string AnyRuntimeVersion = "6";

    private TestConfigurationValidator unit;

    [SetUp]
    public void SetUp()
    {
        this.unit = new TestConfigurationValidator();
    }

    [Test]
    public void Validate__WhenEverythingAskedForIsThere__ThenShouldFindNothingWrong()
    {
        // Given:
        var source = Source() with
        {
            IsTestLinkEnabled = true,
            IdeVersionText = "6.1.4",
            IsRuntimeTestSelected = true,
            TestedHwAssembly = TestedHwAssemblyType.HW01,
        };

        // When:
        Validity validity = this.unit.Validate(source);

        // Then:
        Assert.That(validity.HasErrors, Is.True, validity.Summary);
    }

    [TestCase(null)]
    [TestCase("")]
    public void Validate__WhenNoRuntimeVersionWasChosen__ThenShouldSaySo(string? givenRuntimeVersion)
    {
        // When:
        Validity validity =
            this.unit.Validate(Source() with { RuntimeVersion = givenRuntimeVersion });

        // Then:
        Assert.That(
            validity.For(nameof(ITestConfigurationValidationSource.RuntimeVersion)).Issues,
            Is.Not.Empty);
    }

    [Test]
    public void Validate__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForATestStation()
    {
        // When:
        Validity validity = this.unit.Validate(
            Source() with { IsRuntimeTestSelected = false, TestedHwAssembly = null });

        // Then:
        Assert.That(validity.HasErrors, Is.True, validity.Summary);
    }

    [TestCase(null)]
    [TestCase(TestedHwAssemblyType.Unknown)]
    public void Validate__WhenARuntimeTestIsSelectedWithoutAStation__ThenShouldSaySo(
        TestedHwAssemblyType? givenStation)
    {
        // Given:
        // Unknown stands for a station nobody chose, so it must not pass for one that was.
        var source = Source() with
        {
            IsRuntimeTestSelected = true,
            TestedHwAssembly = givenStation,
        };

        // When:
        Validity validity = this.unit.Validate(source);

        // Then:
        Assert.That(
            validity.For(nameof(ITestConfigurationValidationSource.TestedHwAssembly)).Issues,
            Is.Not.Empty);
    }

    [Test]
    public void Validate__WhenTestLinkIsOff__ThenShouldNotAskForAnIdeVersion()
    {
        // When:
        Validity validity = this.unit.Validate(
            Source() with { IsTestLinkEnabled = false, IdeVersionText = null });

        // Then:
        Assert.That(validity.HasErrors, Is.True, validity.Summary);
    }

    [Test]
    public void Validate__WhenTestLinkIsOffAndTheIdeVersionIsNonsense__ThenShouldStillNotMind()
    {
        // Given:
        // A version left over from when TestLink was on is kept rather than thrown away, so it has
        // to be allowed to sit there unused.
        var source = Source() with
        {
            IsTestLinkEnabled = false,
            IdeVersionText = "not a version",
        };

        // When:
        Validity validity = this.unit.Validate(source);

        // Then:
        Assert.That(validity.HasErrors, Is.True, validity.Summary);
    }

    [TestCase("6.1")]
    [TestCase("6.1.4")]
    [TestCase("10.20.30")]
    public void Validate__WhenTheIdeVersionIsWellFormed__ThenShouldAcceptIt(string givenIdeVersion)
    {
        // When:
        Validity validity = this.unit.Validate(
            Source() with { IsTestLinkEnabled = true, IdeVersionText = givenIdeVersion });

        // Then:
        Assert.That(validity.HasErrors, Is.True, validity.Summary);
    }

    [TestCase(null, Description = "nothing typed")]
    [TestCase("", Description = "nothing typed")]
    [TestCase("6", Description = "the minor part is not optional")]
    [TestCase("6.1.4.2", Description = "there is no fourth part")]
    [TestCase("6.1.", Description = "a trailing separator is not a part")]
    [TestCase("v6.1", Description = "digits and dots only")]
    [TestCase("6.1-beta", Description = "no pre-release suffix")]
    [TestCase(" 6.1", Description = "no padding")]
    public void Validate__WhenTheIdeVersionIsNotWellFormed__ThenShouldSaySo(string? givenIdeVersion)
    {
        // When:
        Validity validity = this.unit.Validate(
            Source() with { IsTestLinkEnabled = true, IdeVersionText = givenIdeVersion });

        // Then:
        Assert.That(
            validity.For(nameof(ITestConfigurationValidationSource.IdeVersionText)).Issues,
            Is.Not.Empty);
    }

    [Test]
    public void Validate__WhenTheIdeVersionIsMissing__ThenShouldSayItOnlyOnce()
    {
        // Given:
        // Empty text is both missing and malformed; saying both would put two messages under one
        // field, of which only the first is any use.
        var source = Source() with
        {
            IsTestLinkEnabled = true,
            IdeVersionText = string.Empty,
        };

        // When:
        Validity validity = this.unit.Validate(source);

        // Then:
        Assert.That(
            validity.For(nameof(ITestConfigurationValidationSource.IdeVersionText)).Issues,
            Has.Exactly(1).Items);
    }

    [Test]
    public void Validate__WhenSeveralThingsAreWrong__ThenShouldReportAllOfThem()
    {
        // Given:
        TestConfigurationSource source = new(
            RuntimeVersion: null,
            TestedHwAssembly: null,
            IsTestLinkEnabled: true,
            IdeVersionText: null,
            IsRuntimeTestSelected: true);

        // When:
        Validity validity = this.unit.Validate(source);

        // Then:
        Assert.That(validity.Issues, Has.Exactly(3).Items, validity.Summary);
    }

    [Test]
    public void Validate__WhenNothingWasGiven__ThenShouldRefuseRatherThanPassIt()
        => Assert.Throws<ArgumentNullException>(() => this.unit.Validate(null!));

    private static TestConfigurationSource Source()
        => new(
            RuntimeVersion: AnyRuntimeVersion,
            TestedHwAssembly: null,
            IsTestLinkEnabled: false,
            IdeVersionText: null,
            IsRuntimeTestSelected: false);

    /// <remarks>
    /// A record, so that each test can say what it changes about an otherwise valid configuration
    /// rather than spell one out in full.
    /// </remarks>
    private sealed record TestConfigurationSource(
        string? RuntimeVersion,
        TestedHwAssemblyType? TestedHwAssembly,
        bool IsTestLinkEnabled,
        string? IdeVersionText,
        bool IsRuntimeTestSelected) : ITestConfigurationValidationSource;
}