namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using System.Linq;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

using TestConfigurationComponent = Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components.TestConfiguration;

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

    private TestDiscoveryState testSelection = new([]);
    private TestConfigurationState configuration = new();

    private Mock<IAppSettingsStore> appSettingsStore;
    private Mock<ITestDiscoveryStore> testDiscoveryStore;
    private Mock<IDispatcher> dispatcher;

    [SetUp]
    public void SetUp()
    {
        var configurationState = new Mock<IState<TestConfigurationState>>();
        configurationState.SetupGet(state => state.Value).Returns(() => this.configuration);

        this.testDiscoveryStore = new Mock<ITestDiscoveryStore>();
        this.testDiscoveryStore.SetupGet(store => store.Current).Returns(() => this.testSelection);

        this.appSettingsStore = new Mock<IAppSettingsStore>();
        this.appSettingsStore
            .SetupGet(store => store.Current)
            .Returns(new AppSettingsState(@"C:\Ide"));

        this.dispatcher = new Mock<IDispatcher>();

        this.Services.AddSingleton(configurationState.Object);
        this.Services.AddSingleton(this.testDiscoveryStore.Object);
        this.Services.AddSingleton(this.appSettingsStore.Object);
        this.Services.AddSingleton(this.dispatcher.Object);
        this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenTheConfiguratorIsShown__ThenShouldOfferTheInstalledRuntimeVersions()
    {
        // When:
        var component = this.RenderConfigurator();

        // Then:
        // The versions plus the empty choice standing for none of them.
        Assert.That(component.FindAll($"{RuntimeVersionSelector} option"), Has.Exactly(3).Items);
    }

    [Test]
    public void Render__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForATestStation()
    {
        // Given:
        // Only a runtime test runs against hardware, so for anything else the field is not shown at
        // all rather than shown and ignored.
        this.GivenSelectedTestCase(TestType.ApplicationTest);

        // When:
        var component = this.RenderConfigurator();

        // Then:
        Assert.That(component.FindAll(TestStationSelector), Is.Empty);
    }

    [Test]
    public void Render__WhenARuntimeTestIsSelected__ThenShouldAskForATestStation()
    {
        // Given:
        this.GivenSelectedTestCase(TestType.RuntimeTest);

        // When:
        var component = this.RenderConfigurator();

        // Then:
        Assert.That(component.FindAll(TestStationSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void Render__WhenTheResultDoesNotGoToTestLink__ThenShouldNotAskForTheIdeVersion()
    {
        // When:
        var component = this.RenderConfigurator();

        // Then:
        Assert.That(component.FindAll(IdeVersionSelector), Is.Empty);
    }

    [Test]
    public void OnTestLinkEnabled__WhenTheResultIsToGoToTestLink__ThenShouldAskForTheIdeVersion()
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
    public void OnTestLinkDisabled__WhenTheResultIsNotToGoToTestLinkAfterAll__ThenShouldStopAsking()
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
    public void OnIdeVersionTyped__WhenWhatWasTypedIsNotAVersion__ThenShouldSaySo()
    {
        // Given:
        var component = this.RenderConfigurator();
        component.Find(TestLinkSelector).Change(true);

        // When:
        component.Find(IdeVersionSelector).Input("nonsense");

        // Then:
        Assert.That(component.FindAll(".property-grid-row-message.is-error"), Is.Not.Empty);
    }

    [Test]
    public void OnAppSettingsChanged__WhenTheInstallFolderMoves__ThenShouldOfferWhatIsInstalledThere()
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
    public void OnAppSettingsChanged__WhenTheChosenVersionIsNotInstalledThere__ThenShouldClearIt()
    {
        // Given:
        // Pointing the install folder somewhere else can leave behind a version that is not
        // installed there. Keeping it would let a run start against an installation that is not on
        // the machine, while the combo box shows nothing chosen.
        this.configuration = new TestConfigurationState() with { RuntimeVersion = "6" };

        var component = this.RenderConfigurator();

        // When:
        this.RaiseAppSettingsChanged(component);

        // Then:
        this.dispatcher.Verify(
            d => d.Dispatch(
                It.Is<DataChangedAction>(action =>
                    action.NewRuntimeVersion != null && action.NewRuntimeVersion.Value == null)));
    }

    [Test]
    public void OnAppSettingsChanged__WhenTheChosenVersionIsInstalledThereToo__ThenShouldKeepIt()
    {
        // Given:
        this.configuration = new TestConfigurationState() with { RuntimeVersion = "6" };

        var component = this.RenderConfigurator();

        // When:
        this.RaiseAppSettingsChanged(component);

        // Then:
        this.dispatcher.Verify(
            d => d.Dispatch(It.Is<DataChangedAction>(action => action.NewRuntimeVersion != null)),
            Times.Never);
    }

    [Test]
    public void Render__WhenTheConfiguratorIsShown__ThenShouldSayWhetherTheConfigurationCanBeRunWith()
    {
        // Given:
        // Nothing has been chosen, and the state comes back from the browser without the answer,
        // which is not remembered with it.
        this.configuration = new TestConfigurationState();

        // When:
        this.RenderConfigurator();

        // Then:
        this.dispatcher.Verify(d => d.Dispatch(It.Is<DataChangedAction>(action => !action.IsValid)));
    }

    [Test]
    public void OnRuntimeVersionChosen__WhenTheLastMissingValueIsGiven__ThenShouldSayItCanBeRunWith()
    {
        // Given:
        // The run needs a runtime version and nothing else while no runtime test is selected, so
        // choosing one is what makes this configuration runnable.
        var component = this.RenderConfigurator();

        // When:
        component.Find(RuntimeVersionSelector).Change("6");

        // Then:
        this.dispatcher.Verify(d => d.Dispatch(It.Is<DataChangedAction>(action => action.IsValid)));
    }

    [Test]
    public void OnTestSelectionChanged__WhenARuntimeTestIsPicked__ThenShouldSayItCannotBeRunWith()
    {
        // Given:
        // A runtime test runs against a station, which nothing has been chosen for, so the same
        // configuration that was runnable a moment ago is not any more.
        this.configuration = new TestConfigurationState() with { RuntimeVersion = "6" };

        var component = this.RenderConfigurator();
        this.dispatcher.Invocations.Clear();

        // When:
        this.GivenSelectedTestCase(TestType.RuntimeTest);
        this.RaiseTestSelectionChanged(component);

        // Then:
        this.dispatcher.Verify(d => d.Dispatch(It.Is<DataChangedAction>(action => !action.IsValid)));
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
        this.testDiscoveryStore.Raise(store => store.Changed += null);

        component.WaitForState(() => true);
    }

    private void GivenSelectedTestCase(TestType testType)
        => this.testSelection = new TestDiscoveryState(
            [new TestCaseEntity(testType, id: "1", name: "Test", executionPath: "Suite.Fixture.Test")]);
}