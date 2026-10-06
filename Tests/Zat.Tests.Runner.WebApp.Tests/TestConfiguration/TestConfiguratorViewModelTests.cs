namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Validation;
using Zat.Z2xxTests.Common.Model;

[TestFixture]
public class TestConfiguratorViewModelTests
{
    private TestConfigurationStoreFake store;
    private TestConfigurationViewModel unit;

    [SetUp]
    public void SetUp()
    {
        // The real rules rather than a mock: what the view model shows is the rules applied to what
        // is being typed, so a stand-in would leave the interesting part untested.
        this.store = new TestConfigurationStoreFake();

        this.unit = new TestConfigurationViewModel(
            this.store,
            new Mock<IAppSettingsStore>().Object,
            new Mock<ILogger<LogSources.App>>().Object,
            this.store.Dispatcher.Object);
    }

    [TearDown]
    public void TearDown()
        => this.unit.Dispose();

    [Test]
    public void Load__WhenConfigurationIsOpened__ThenShouldShowWhatItHolds()
    {
        // Given:
        var state = new TestConfigurationState(
            IsTestLinkReportEnabled: true,
            IsBetaVersion: true,
            IsDebugModeEnabled: true,
            IdeVersion: new Version(6, 1),
            TestedHwAssembly: HwAssemblyType.HW01,
            RuntimeVersion: "6",
            RuntimeReleaseDate: "30.09.2026",
            IdeReleaseDate: "01.10.2026",
            BetaVersion: "3");

        // When:
        this.store.ChangeTo(state);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.RuntimeVersion, Is.EqualTo("6"));
            Assert.That(this.unit.TestedHwAssembly, Is.EqualTo(HwAssemblyType.HW01));
            Assert.That(this.unit.IsTestLinkReportEnabled, Is.True);
            Assert.That(this.unit.IdeVersion, Is.EqualTo(new Version(6, 1)));
            Assert.That(this.unit.IdeReleaseDate, Is.EqualTo("01.10.2026"));
            Assert.That(this.unit.RuntimeReleaseDate, Is.EqualTo("30.09.2026"));
            Assert.That(this.unit.IsBetaVersion, Is.True);
            Assert.That(this.unit.BetaVersion, Is.EqualTo("3"));
            Assert.That(this.unit.IsDebugModeEnabled, Is.True);
        }
    }

    [Test]
    public void ValidityFor__WhenConfigurationHasJustBeenOpened__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:
        // An empty configuration cannot be run with, so the tester is told so before touching it.

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.HasErrors, Is.True);
            Assert.That(this.store.Value.HasErrors, Is.True);
            Assert.That(
                this.unit.GetValidity(nameof(TestConfigurationViewModel.RuntimeVersion))?.HasErrors,
                Is.True);
        }
    }

    [Test]
    public void ValidityFor__WhenFieldIsNotAskedFor__ThenShouldNotHoldRunBack()
    {
        // Given:
        // Neither TestLink nor a runtime test is chosen, so the IDE version and the test station are
        // hidden and nobody could fill them in.
        this.store.ChangeTo(new TestConfigurationState() with { RuntimeVersion = "6" });

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.HasErrors, Is.False);
            Assert.That(this.store.Value.HasErrors, Is.False);
        }
    }

    [Test]
    public void ValidityFor__WhenTestLinkIsTurnedOn__ThenShouldAskForIdeVersion()
    {
        // Given:
        this.store.ChangeTo(new TestConfigurationState() with { RuntimeVersion = "6" });

        // When:
        this.unit.IsTestLinkReportEnabled = true;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.HasErrors, Is.True);
            Assert.That(
                this.unit.GetValidity(nameof(TestConfigurationViewModel.IdeVersion))?.HasErrors,
                Is.True);
        }
    }

    [Test]
    public void ValidityFor__WhenChangeOfFieldIsAnnounced__ThenShouldAlreadySayWhatIsWrongWithNewValue()
    {
        // Given:
        // A control redraws as soon as it hears of the change, and while another field stays wrong
        // nothing else would make it redraw again.
        this.unit.IsTestLinkReportEnabled = true;

        Validity? announced = null;
        this.unit.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(TestConfigurationViewModel.RuntimeVersion))
            {
                announced = this.unit.GetValidity(nameof(TestConfigurationViewModel.RuntimeVersion));
            }
        };

        // When:
        this.unit.RuntimeVersion = "6";

        // Then:
        Assert.That(announced?.HasErrors, Is.False);
    }

    [Test]
    public void ValidityFor__WhenTesterHasBeenToField__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:

        // When:
        // TODO

        // Then:
        // TODO
    }

    [Test]
    public void SetRuntimeVersion__WhenChoiceIsCleared__ThenShouldHoldNothingRatherThanEmptyText()
    {
        // Given:
        // The empty choice of the combo box arrives as an empty string, which is not a version
        // named after nothing.

        // When:
        // TODO

        // Then:
        // TODO
    }

    [Test]
    public void IsTestedHwAssemblyShown__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForStation()
    {
        // TODO
    }

    [Test]
    public void SetRuntimeTestSelected__WhenRuntimeTestIsPicked__ThenShouldStartAskingForStation()
    {
        // TODO
    }

    [Test]
    public void SetRuntimeTestSelected__WhenRuntimeTestIsPicked__ThenShouldMindMissingStation()
    {
        // TODO
    }

    [Test]
    public void IsIdeVersionShown__WhenTestLinkIsTurnedOn__ThenShouldStartAskingForIdeVersion()
    {
        // TODO
    }

    [Test]
    public void SetTestLinkEnabled__WhenItIsTurnedOffAgain__ThenShouldKeepWhatWasTyped()
    {
        // TODO
    }

    [Test]
    public void IdeVersion__WhenTextIsVersion__ThenShouldHandItOverToBeKept()
    {
        // TODO
    }

    [Test]
    public void IdeVersion__WhileTextIsBeingTyped__ThenShouldHoldNothingToKeepYet()
    {
        // TODO
    }

    [TestCase("01.10.2026", false)]
    [TestCase("1.10.2026", false)]
    [TestCase("1.1.2026", false)]
    [TestCase("", false)]
    [TestCase("yesterday", true)]
    [TestCase("2026-10-01", true)]
    [TestCase("1.10.26", true)]
    [TestCase("31.13.2026", true)]
    public void ValidityFor__WhenTestLinkIsOn_AndIdeReleaseDateIsTyped__ThenShouldMindTextThatIsNotDate(
        string givenText, bool expectedHasErrors)
    {
        // Given:
        this.unit.IsTestLinkReportEnabled = true;

        // When:
        this.unit.IdeReleaseDate = givenText;

        // Then:
        Assert.That(
            this.unit.GetValidity(nameof(TestConfigurationViewModel.IdeReleaseDate))?.HasErrors,
            Is.EqualTo(expectedHasErrors));
    }

    [TestCase("30.09.2026", false)]
    [TestCase("2026-09-30", true)]
    public void ValidityFor__WhenTestLinkIsOn_AndRuntimeReleaseDateIsTyped__ThenShouldMindTextThatIsNotDate(
        string givenText, bool expectedHasErrors)
    {
        // Given:
        this.unit.IsTestLinkReportEnabled = true;

        // When:
        this.unit.RuntimeReleaseDate = givenText;

        // Then:
        Assert.That(
            this.unit.GetValidity(nameof(TestConfigurationViewModel.RuntimeReleaseDate))?.HasErrors,
            Is.EqualTo(expectedHasErrors));
    }

    [Test]
    public void ValidityFor__WhenTestLinkIsTurnedOff__ThenShouldNoLongerMindWhatIsNotAskedFor()
    {
        // Given:
        this.unit.IsTestLinkReportEnabled = true;
        this.unit.IdeVersion = new Version(1, 2, 3, 4);
        this.unit.IdeReleaseDate = "yesterday";

        // When:
        this.unit.IsTestLinkReportEnabled = false;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.GetValidity(nameof(TestConfigurationViewModel.IdeVersion))?.HasErrors, Is.False);
            Assert.That(this.unit.GetValidity(nameof(TestConfigurationViewModel.IdeReleaseDate))?.HasErrors, Is.False);
            Assert.That(this.unit.HasErrors, Is.False);
        }
    }

    [TestCase(true, "beta", true)]
    [TestCase(true, "3", false)]
    [TestCase(true, "", false)]
    [TestCase(false, "beta", false)]
    public void ValidityFor__WhenBetaNumberIsTyped__ThenShouldMindTextThatIsNotWholeNumberOfBeta(
        bool givenIsBetaVersion, string givenText, bool expectedHasErrors)
    {
        // Given:
        this.unit.IsTestLinkReportEnabled = true;
        this.unit.IsBetaVersion = givenIsBetaVersion;

        // When:
        this.unit.BetaVersion = givenText;

        // Then:
        Assert.That(
            this.unit.GetValidity(nameof(TestConfigurationViewModel.BetaVersion))?.HasErrors,
            Is.EqualTo(expectedHasErrors));
    }

    [Test]
    public void ValidityFor__WhenBetaIsTurnedOff__ThenShouldNoLongerMindBetaNumber()
    {
        // Given:
        this.unit.IsTestLinkReportEnabled = true;
        this.unit.IsBetaVersion = true;
        this.unit.BetaVersion = "beta";

        // When:
        this.unit.IsBetaVersion = false;

        // Then:
        Assert.That(this.unit.HasErrors, Is.False);
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
    public void RuntimeVersionsNote__WhenInstallFolderCannotBeRead__ThenShouldPointAtSettings()
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