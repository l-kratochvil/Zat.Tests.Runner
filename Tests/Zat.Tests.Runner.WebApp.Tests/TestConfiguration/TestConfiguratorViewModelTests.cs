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
        var state = new TestConfigurationState() with
        {
            IsTestLinkReportEnabled = true,
            IdeVersion = new Version(6, 1),
            RuntimeVersion = "6",
            TestedHwAssembly = HwAssemblyType.HW01,
        };

        // When:
        this.store.ChangeTo(state);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.RuntimeVersion, Is.EqualTo("6"));
            Assert.That(this.unit.TestedHwAssembly, Is.EqualTo(HwAssemblyType.HW01));
            Assert.That(this.unit.IsTestLinkReportEnabled, Is.True);
            Assert.That(this.unit.IdeVersion, Is.EqualTo(new Version(6, 1)));
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