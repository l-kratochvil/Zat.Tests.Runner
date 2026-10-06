namespace Zat.Tests.Runner.WebApp.Tests.TestExecution;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Features.TestExecution.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

/// <summary>
/// What the tester can do with the test run and what keeps a new one from starting.
/// </summary>
/// <remarks>
/// Whether a configuration can be run with is not asked here: the view model only reads the answer
/// the configurator put into the state, and the rules behind it are exercised in
/// <see cref="TestConfiguratorViewModelTests"/>.
/// </remarks>
[TestFixture]
public class TestExecutionViewModelTests
{
    private const string StartLabel = "Start";
    private const string StopLabel = "Stop";
    private const string StoppingLabel = "Stopping…";

    private Mock<ITestRunnerEngine> testRunnerEngine;
    private Mock<ITestDiscoveryStore> testDiscoveryStore;
    private TestConfigurationStoreFake configuration;

    private TestDiscoveryState testSelection;
    private TestRunState testRunState;

    private TestExecutionViewModel unit;

    [SetUp]
    public void SetUp()
    {
        this.testSelection = new TestDiscoveryState([]);
        this.testRunState = TestRunState.Stopped;

        this.testRunnerEngine = new Mock<ITestRunnerEngine>();
        this.testRunnerEngine.SetupGet(engine => engine.State).Returns(() => this.testRunState);

        this.testDiscoveryStore = new Mock<ITestDiscoveryStore>();
        this.testDiscoveryStore.SetupGet(store => store.Current).Returns(() => this.testSelection);

        this.configuration = new TestConfigurationStoreFake();

        this.unit = new TestExecutionViewModel(
            this.testRunnerEngine.Object,
            [],
            this.configuration,
            this.testDiscoveryStore.Object);
    }

    [TearDown]
    public void TearDown()
        => this.unit.Dispose();

    [TestCase(TestRunState.Stopped, StartLabel)]
    [TestCase(TestRunState.Running, StopLabel)]
    [TestCase(TestRunState.Stopping, StoppingLabel)]
    public void CurrentCommand__WhenTestRunIsInState__ThenShouldOfferWhatCanBeDoneWithIt(
        TestRunState givenState,
        string expectedLabel)
    {
        // Given:
        this.testRunState = givenState;

        // When:
        var command = this.unit.CurrentCommand;

        // Then:
        Assert.That(command.DisplayText, Is.EqualTo(expectedLabel));
    }

    [TestCase(TestRunState.Stopped, true)]
    [TestCase(TestRunState.Running, true)]
    [TestCase(TestRunState.Stopping, false)]
    public void CurrentCommand__WhenSelectionAndConfigurationAreRunnable__ThenShouldBeExecutableUnlessStopping(
        TestRunState givenState,
        bool expectedCanExecute)
    {
        // Given:
        this.GivenARunnableConfiguration();
        this.testRunState = givenState;

        // When:
        var canExecute = this.unit.CurrentCommand.CanExecute(null);

        // Then:
        Assert.That(canExecute, Is.EqualTo(expectedCanExecute));
    }

    [Test]
    public void CurrentCommand__WhenStopIsExecuted__ThenShouldStopTestRun()
    {
        // Given:
        this.testRunState = TestRunState.Running;

        // When:
        this.unit.CurrentCommand.Execute(null);

        // Then:
        this.testRunnerEngine.Verify(engine => engine.StopTestRun(), Times.Once);
    }

    [Test]
    public void ExecutionBlockingReasons__WhenNoTestIsSelected__ThenShouldSaySo()
    {
        // Given:
        // A run of nothing is not a run, and the reason is said where the tester starts the run
        // rather than in the explorer beside it.
        this.configuration.Value = new TestConfigurationState { HasErrors = false };

        // When:
        var reasons = this.unit.ExecutionBlockingReasons;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(reasons, Is.EqualTo(new[] { "No tests selected" }));
            Assert.That(this.unit.CurrentCommand.CanExecute(null), Is.False);
        }
    }

    [Test]
    public void ExecutionBlockingReasons__WhenConfigurationCannotBeRunWith__ThenShouldSaySo()
    {
        // Given:
        // What is wrong with it is not said here: the configurator is beside the button and says it
        // field by field.
        this.configuration.Value = new TestConfigurationState { HasErrors = true };
        this.GivenSelectedTestCase();

        // When:
        var reasons = this.unit.ExecutionBlockingReasons;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(reasons, Is.EqualTo(new[] { "Configuration has errors" }));
            Assert.That(this.unit.CurrentCommand.CanExecute(null), Is.False);
        }
    }

    [Test]
    public void ExecutionBlockingReasons__WhenSelectionAndConfigurationAreBothThere__ThenShouldBeEmpty()
    {
        // Given:
        this.GivenARunnableConfiguration();

        // When:
        var reasons = this.unit.ExecutionBlockingReasons;

        // Then:
        Assert.That(reasons, Is.Empty);
    }

    [Test]
    public void OnTestRunStateChanged__WhenAnotherClientStartsTestRun__ThenShouldAnnounceCurrentCommand()
    {
        // Given:
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.testRunState = TestRunState.Running;
        this.testRunnerEngine.Raise(engine => engine.StateChanged += null, TestRunState.Running);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(announced, Does.Contain(nameof(TestExecutionViewModel.CurrentCommand)));
            Assert.That(this.unit.CurrentCommand.DisplayText, Is.EqualTo(StopLabel));
        }
    }

    [Test]
    public void OnTestSelectionChanged__WhenTestCaseIsSelected__ThenShouldAnnounceBlockingReasons()
    {
        // Given:
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.GivenSelectedTestCase();
        this.testDiscoveryStore.Raise(store => store.Changed += null, this.testSelection);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(announced, Does.Contain(nameof(TestExecutionViewModel.ExecutionBlockingReasons)));
            Assert.That(this.unit.TestsSelected, Is.True);
        }
    }

    [Test]
    public void OnConfigurationChanged__WhenConfigurationTurnsRunnable__ThenShouldAnnounceBlockingReasons()
    {
        // Given:
        this.configuration.Value = new TestConfigurationState { HasErrors = true };
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.configuration.ChangeTo(new TestConfigurationState { HasErrors = false });

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(announced, Does.Contain(nameof(TestExecutionViewModel.ExecutionBlockingReasons)));
            Assert.That(this.unit.ConfigurationHasErrors, Is.False);
        }
    }

    [Test]
    public void Dispose__WhenTestRunStateChangesAfterwards__ThenShouldAnnounceNothing()
    {
        // Given:
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.unit.Dispose();
        this.testRunnerEngine.Raise(engine => engine.StateChanged += null, TestRunState.Running);
        this.testDiscoveryStore.Raise(store => store.Changed += null, this.testSelection);
        this.configuration.ChangeTo(new TestConfigurationState { HasErrors = true });

        // Then:
        Assert.That(announced, Is.Empty);
    }

    private List<string?> RecordAnnouncedProperties()
    {
        var announced = new List<string?>();
        this.unit.PropertyChanged += (_, e) => announced.Add(e.PropertyName);
        return announced;
    }

    private void GivenARunnableConfiguration()
    {
        this.configuration.Value = new TestConfigurationState { HasErrors = false };
        this.GivenSelectedTestCase();
    }

    private void GivenSelectedTestCase()
        => this.testSelection = new TestDiscoveryState(
            [new TestCaseEntity(TestType.Application, id: "1", name: "Test", executionPath: "Suite.Fixture.Test")]);
}