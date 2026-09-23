namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Z2xxTests.Common.Model;

[TestFixture]
public class TestConfiguratorViewModelTests
{
    private TestConfigurationViewModel unit;

    [SetUp]
    public void SetUp()
    {
        // The real rules rather than a mock: what the view model shows is the rules applied to what
        // is being typed, so a stand-in would leave the interesting part untested.
        // TODO: Setup the viewmodel correctly
        this.unit = new TestConfigurationViewModel(null, null, null, null);
    }

    [Test]
    public void Load__WhenAConfigurationIsOpened__ThenShouldShowWhatItHolds()
    {
        // Given:
        var state = new TestConfigurationState(
            IsTestLinkReportEnabled: true,
            IdeVersion: new Version(6, 1),
            RuntimeVersion: "6",
            TestedHwAssembly: HwAssemblyType.HW01);

        // When:
        this.unit.Initialize();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.RuntimeVersion, Is.EqualTo("6"));
            Assert.That(this.unit.TestedHwAssembly, Is.EqualTo(HwAssemblyType.HW01));
            Assert.That(this.unit.IsTestLinkReportEnabled, Is.True);
            // TODO: Assert.That(this.unit.IdeVersion, Is.EqualTo("6.1"));
        }
    }

    [Test]
    public void ValidityFor__WhenAConfigurationHasJustBeenOpened__ThenShouldSayNothingAboutItYet()
    {
        // Given:
        // An empty configuration nobody has touched is not a mistake anyone made yet.
        this.unit.Initialize();

        // Then:
        Assert.That(this.unit.HasErrors, Is.False);
    }

    [Test]
    public void ValidityFor__WhenTheTesterHasBeenToAField__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:
        this.unit.Initialize();

        // When:
        // TODO

        // Then:
        // TODO
    }

    [Test]
    public void ValidityFor__WhenAnotherFieldWasTouched__ThenShouldStillSayNothingAboutThisOne()
    {
        // Given:
        this.unit.Initialize();

        // When:
        // TODO

        // Then:
        // TODO
    }

    [Test]
    public void SetRuntimeVersion__WhenTheChoiceIsCleared__ThenShouldHoldNothingRatherThanEmptyText()
    {
        // Given:
        // The empty choice of the combo box arrives as an empty string, which is not a version
        // named after nothing.
        this.unit.Initialize();

        // When:
        // TODO

        // Then:
        // TODO
    }

    [Test]
    public void IsTestedHwAssemblyShown__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForAStation()
    {
        // TODO
    }

    [Test]
    public void SetRuntimeTestSelected__WhenARuntimeTestIsPicked__ThenShouldStartAskingForAStation()
    {
        // TODO
    }

    [Test]
    public void SetRuntimeTestSelected__WhenARuntimeTestIsPicked__ThenShouldMindTheMissingStation()
    {
        // TODO
    }

    [Test]
    public void IsIdeVersionShown__WhenTestLinkIsTurnedOn__ThenShouldStartAskingForTheIdeVersion()
    {
        // TODO
    }

    [Test]
    public void SetTestLinkEnabled__WhenItIsTurnedOffAgain__ThenShouldKeepWhatWasTyped()
    {
        // TODO
    }

    [Test]
    public void IdeVersion__WhenTheTextIsAVersion__ThenShouldHandItOverToBeKept()
    {
        // TODO
    }

    [Test]
    public void IdeVersion__WhileTheTextIsBeingTyped__ThenShouldHoldNothingToKeepYet()
    {
        // TODO
    }

    [Test]
    public void RuntimeVersionsNote__WhenVersionsAreInstalled__ThenShouldHaveNothingToSay()
    {
        // TODO
    }

    [Test]
    public void RuntimeVersionsNote__WhenNoneIsInstalled__ThenShouldSayThereIsNothingToChooseFrom()
    {
        // TODO
    }

    [Test]
    public void RuntimeVersionsNote__WhenTheInstallFolderCannotBeRead__ThenShouldPointAtTheSettings()
    {
        // Given:
        // Nowhere to look is the tester's to fix in the settings, and is a different thing from an
        // install folder holding nothing yet.
        // TODO
    }

    [Test]
    public void Load__WhenNothingWasGiven__ThenShouldRefuseRatherThanEditNothing()
    {
        // TODO
    }
}