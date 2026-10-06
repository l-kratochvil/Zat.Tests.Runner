namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using System.Linq;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Tests.TestDiscovery;

using TestConfigurationComponent = Features.TestConfiguration.Components.TestConfiguration;

/// <summary>
/// What the configurator asks for, which is the part of it that is not simply bound to a field.
/// </summary>
/// <remarks>
/// Only the rules about which fields appear at all are exercised here; what each field does with
/// what is typed into it belongs to <see cref="TestConfiguratorViewModelTests"/>, which can say it
/// without rendering anything.
/// </remarks>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestConfiguratorTests : Bunit.TestContext
{
    private const string RuntimeVersionSelector = ".input-runtime-version";
    private const string TestStationSelector = ".input-hw-assembly-type";
    private const string TestLinkSelector = ".input-report-to-testlink";
    private const string IdeVersionSelector = ".input-ide-version";
    private const string IdeReleaseDateSelector = ".input-ide-release-date";
    private const string RuntimeReleaseDateSelector = ".input-runtime-release-date";
    private const string BetaSelector = ".input-is-beta-version";
    private const string BetaNumberSelector = ".input-beta-version";
    private const string DebugModeSelector = ".input-debug-mode";

    private const string GivenTestCasePath = "Suite.Fixture.Test";

    private TestDiscoveryState testSelection = new();
    private IReadOnlyList<TestSuiteEntity> loadedTestSuites = [];

    private TestConfigurationStoreFake store;
    private Mock<IAppSettingsStore> appSettingsStore;
    private Mock<IState<TestDiscoveryState>> testSelectionState;

    [SetUp]
    public void SetUp()
    {
        this.store = new TestConfigurationStoreFake();

        this.testSelectionState = new Mock<IState<TestDiscoveryState>>();
        this.testSelectionState.SetupGet(state => state.Value).Returns(() => this.testSelection);

        var testRunnerStore = new Mock<INUnitTestRunnerStore>();
        testRunnerStore.SetupGet(store => store.LoadedTestSuites).Returns(() => this.loadedTestSuites);

        this.appSettingsStore = new Mock<IAppSettingsStore>();
        this.appSettingsStore
            .SetupGet(store => store.Current)
            .Returns(new AppSettingsState(@"C:\Ide"));

        this.Services.AddSingleton<IState<TestConfigurationState>>(this.store);
        this.Services.AddSingleton(this.testSelectionState.Object);
        this.Services.AddSingleton(testRunnerStore.Object);
        this.Services.AddSingleton(this.appSettingsStore.Object);
        this.Services.AddSingleton(this.store.Dispatcher.Object);
        this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);
        this.Services.AddSingleton(new Mock<ILogger<LogSources.App>>().Object);
        this.Services.AddScoped<TestConfigurationViewModel>();
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenConfiguratorIsShown__ThenShouldOfferInstalledRuntimeVersions()
    {
        // When:
        var component = this.RenderConfigurator();

        // Then:
        // The versions plus the empty choice standing for none of them.
        Assert.That(component.FindAll($"{RuntimeVersionSelector} option"), Has.Exactly(3).Items);
    }

    [Test]
    public void Render__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForTestStation()
    {
        // Given:
        // Only a runtime test runs against hardware, so for anything else the field is not shown at
        // all rather than shown and ignored.
        this.GivenSelectedTestCase(TestType.Application);

        // When:
        var component = this.RenderConfigurator();

        // Then:
        Assert.That(component.FindAll(TestStationSelector), Is.Empty);
    }

    [Test]
    public void Render__WhenRuntimeTestIsSelected__ThenShouldAskForTestStation()
    {
        // Given:
        this.GivenSelectedTestCase(TestType.Runtime);

        // When:
        var component = this.RenderConfigurator();

        // Then:
        Assert.That(component.FindAll(TestStationSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void Render__WhenResultDoesNotGoToTestLink__ThenShouldNotAskForIdeVersion()
    {
        // When:
        var component = this.RenderConfigurator();

        // Then:
        Assert.That(component.FindAll(IdeVersionSelector), Is.Empty);
    }

    [Test]
    public void Render__WhenResultDoesNotGoToTestLink__ThenShouldAskOnlyForDebugMode()
    {
        // When:
        var component = this.RenderConfigurator();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(IdeReleaseDateSelector), Is.Empty);
            Assert.That(component.FindAll(RuntimeReleaseDateSelector), Is.Empty);
            Assert.That(component.FindAll(BetaSelector), Is.Empty);
            Assert.That(component.FindAll(BetaNumberSelector), Is.Empty);
            Assert.That(component.FindAll(DebugModeSelector), Has.Exactly(1).Items);
        }
    }

    [Test]
    public void OnTestLinkEnabled__WhenResultIsToGoToTestLink__ThenShouldAskForWhatIsNotedThere()
    {
        // Given:
        var component = this.RenderConfigurator();

        // When:
        component.Find(TestLinkSelector).Change(true);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(IdeReleaseDateSelector), Has.Exactly(1).Items);
            Assert.That(component.FindAll(RuntimeReleaseDateSelector), Has.Exactly(1).Items);
            Assert.That(component.FindAll(BetaSelector), Has.Exactly(1).Items);
            Assert.That(component.FindAll(BetaNumberSelector), Is.Empty);
        }
    }

    [Test]
    public void OnBetaEnabled__WhenTestedBuildIsBeta__ThenShouldAskForBetaNumber()
    {
        // Given:
        var component = this.RenderConfigurator();
        component.Find(TestLinkSelector).Change(true);

        // When:
        component.Find(BetaSelector).Change(true);

        // Then:
        Assert.That(component.FindAll(BetaNumberSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void OnTestLinkEnabled__WhenResultIsToGoToTestLink__ThenShouldAskForIdeVersion()
    {
        // Given:
        // The version is what the result is filed under, so it is asked for exactly when there is
        // somewhere to file it.
        var component = this.RenderConfigurator();

        // When:
        component.Find(TestLinkSelector).Change(true);

        // Then:
        Assert.That(component.FindAll(IdeVersionSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void OnTestLinkDisabled__WhenResultIsNotToGoToTestLinkAfterAll__ThenShouldStopAsking()
    {
        // Given:
        var component = this.RenderConfigurator();
        component.Find(TestLinkSelector).Change(true);

        // When:
        component.Find(TestLinkSelector).Change(false);

        // Then:
        Assert.That(component.FindAll(IdeVersionSelector), Is.Empty);
    }

    [Test]
    public void OnIdeVersionTyped__WhenWhatWasTypedIsNotVersion__ThenShouldSaySo()
    {
        // Given:
        var component = this.RenderConfigurator();
        component.Find(TestLinkSelector).Change(true);

        // When:
        component.Find(IdeVersionSelector).Change("nonsense");

        // Then:
        Assert.That(component.FindAll(".property-grid-row-message.is-error"), Is.Not.Empty);
    }

    [Test]
    public void OnAppSettingsChanged__WhenInstallFolderMoves__ThenShouldOfferWhatIsInstalledThere()
    {
        // Given:
        var component = this.RenderConfigurator();

        // When:
        this.RaiseAppSettingsChanged(component);

        // Then:
        Assert.That(
            component.FindAll($"{RuntimeVersionSelector} option").Select(option => option.TextContent.Trim()),
            Is.EqualTo(new[] { "—", "9" }));
    }

    [Test]
    public void OnAppSettingsChanged__WhenChosenVersionIsNotInstalledThere__ThenShouldClearIt()
    {
        // Given:
        // Pointing the install folder somewhere else can leave behind a version that is not
        // installed there. Keeping it would let a run start against an installation that is not on
        // the machine, while the combo box shows nothing chosen.
        this.store.Value = new TestConfigurationState() with { RuntimeVersion = "6" };

        var component = this.RenderConfigurator();

        // When:
        this.RaiseAppSettingsChanged(component);

        // Then:
        this.store.Dispatcher.Verify(
            d => d.Dispatch(
                It.Is<DataChangedAction>(action =>
                    action.NewRuntimeVersion != null && action.NewRuntimeVersion.Value == null)));
    }

    [Test]
    public void OnAppSettingsChanged__WhenChosenVersionIsInstalledThereToo__ThenShouldKeepIt()
    {
        // Given:
        this.store.Value = new TestConfigurationState() with { RuntimeVersion = "6" };

        var component = this.RenderConfigurator();

        // When:
        this.RaiseAppSettingsChanged(component);

        // Then:
        this.store.Dispatcher.Verify(
            d => d.Dispatch(It.Is<DataChangedAction>(action => action.NewRuntimeVersion != null)),
            Times.Never);
    }

    [Test]
    public void Render__WhenConfiguratorIsShown__ThenShouldSayWhetherConfigurationCanBeRunWith()
    {
        // Given:
        // Nothing has been chosen, and the state comes back from the browser without the answer,
        // which is not remembered with it.
        this.store.Value = new TestConfigurationState();

        // When:
        this.RenderConfigurator();

        // Then:
        // TODO
    }

    [Test]
    public void OnRuntimeVersionChosen__WhenLastMissingValueIsGiven__ThenShouldSayItCanBeRunWith()
    {
        // Given:
        // The run needs a runtime version and nothing else while no runtime test is selected, so
        // choosing one is what makes this configuration runnable.
        var component = this.RenderConfigurator();

        // When:
        component.Find(RuntimeVersionSelector).Change("6");

        // Then:
        // TODO
    }

    [Test]
    public void OnTestSelectionChanged__WhenRuntimeTestIsPicked__ThenShouldSayItCannotBeRunWith()
    {
        // Given:
        // A runtime test runs against a station, which nothing has been chosen for, so the same
        // configuration that was runnable a moment ago is not any more.
        this.store.Value = new TestConfigurationState() with { RuntimeVersion = "6" };

        var component = this.RenderConfigurator();
        this.store.Dispatcher.Invocations.Clear();

        // When:
        this.GivenSelectedTestCase(TestType.Runtime);
        this.RaiseTestSelectionChanged(component);

        // Then:
        // TODO
    }

    private IRenderedComponent<TestConfigurationComponent> RenderConfigurator()
        => this.RenderComponent<TestConfigurationComponent>();

    // The settings are one instance shared by everyone connected, so they announce a change on the
    // thread of whoever made it rather than on this circuit's.
    private void RaiseAppSettingsChanged(IRenderedComponent<TestConfigurationComponent> component)
    {
        this.appSettingsStore.Raise(store => store.Changed += null);

        component.WaitForState(() => true);
    }

    private void RaiseTestSelectionChanged(IRenderedComponent<TestConfigurationComponent> component)
    {
        this.testSelectionState.Raise(state => state.StateChanged += null, EventArgs.Empty);

        component.WaitForState(() => true);
    }

    private void GivenSelectedTestCase(TestType testType)
    {
        this.loadedTestSuites = [TestSuites.WithOneTestCase(testType, GivenTestCasePath)];
        this.testSelection = new TestDiscoveryState([GivenTestCasePath]);
    }
}