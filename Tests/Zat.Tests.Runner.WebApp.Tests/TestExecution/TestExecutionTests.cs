namespace Zat.Tests.Runner.WebApp.Tests.TestExecution;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Features.TestExecution.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Tests.TestDiscovery;

using TestExecutionComponent = Zat.Tests.Runner.WebApp.Features.TestExecution.Components.TestExecution;

/// <summary>
/// That the button shows what its view model offers and is redrawn when that changes.
/// </summary>
/// <remarks>
/// What is offered and what stops a test run from starting is decided by the view model and
/// exercised in <see cref="TestExecutionViewModelTests"/>.
/// </remarks>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestExecutionTests : Bunit.TestContext
{
    private const string ButtonSelector = "button";

    private const string StartLabel = "Start";
    private const string StopLabel = "Stop";
    private const string StoppingLabel = "Stopping…";

    private const string GivenTestCasePath = "Suite.Fixture.Test";

    private readonly Mock<ITestRunnerEngine> testRunnerEngine = new();
    private readonly Mock<IState<TestConfigurationState>> configurationState = new();
    private readonly Mock<IState<TestDiscoveryState>> testSelectionState = new();

    private TestConfigurationState configuration = new();
    private TestDiscoveryState testSelection = new();
    private TestRunState testRunState = TestRunState.Stopped;

    [SetUp]
    public void SetUp()
    {
        this.configurationState.SetupGet(state => state.Value).Returns(() => this.configuration);
        this.testSelectionState.SetupGet(state => state.Value).Returns(() => this.testSelection);

        var testTreeStore = new Mock<ITestTreeStore>();
        testTreeStore
            .SetupGet(store => store.TestSuites)
            .Returns([TestSuites.WithOneTestCase(TestType.Application, GivenTestCasePath)]);

        this.testRunnerEngine.SetupGet(engine => engine.State).Returns(() => this.testRunState);

        this.Services.AddSingleton(this.configurationState.Object);
        this.Services.AddSingleton(this.testSelectionState.Object);
        this.Services.AddSingleton(testTreeStore.Object);
        this.Services.AddSingleton(new Mock<IAppPathsProvider>().Object);
        this.Services.AddSingleton(this.testRunnerEngine.Object);
        this.Services.AddScoped<TestExecutionViewModel>();
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenNothingHasBeenClicked__ThenShouldOfferToStartRun()
    {
        // Given:
        this.GivenARunnableConfiguration();

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.That(Label(component), Is.EqualTo(StartLabel));
    }

    [Test]
    public void OnTestRunStateChanged__WhenAnotherClientStartsTestRun__ThenShouldOfferToStopIt()
    {
        // Given:
        this.GivenARunnableConfiguration();

        var component =
            this.RenderComponent<TestExecutionComponent>();

        // When:
        this.testRunState = TestRunState.Running;
        this.testRunnerEngine.Raise(engine => engine.StateChanged += null, TestRunState.Running);

        // Then:
        component.WaitForAssertion(() =>
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Label(component), Is.EqualTo(StopLabel));
                Assert.That(IsDisabled(component), Is.False);
            }
        });
    }

    [Test]
    public void OnConfigurationChanged__WhenConfigurationTurnsRunnable__ThenShouldLetRunStart()
    {
        // Given:
        this.GivenSelectedTestCase();
        this.configuration = new TestConfigurationState { HasErrors = true };

        var component =
            this.RenderComponent<TestExecutionComponent>();

        // When:
        this.configuration = ConfigurationSaidToBeRunnable();
        this.configurationState.Raise(state => state.StateChanged += null, EventArgs.Empty);

        // Then:
        component.WaitForAssertion(() => Assert.That(IsDisabled(component), Is.False));
    }

    [Test]
    public void Render__WhenTestRunIsStopping__ThenShouldNotLetAnythingBeClicked()
    {
        // Given:
        this.GivenARunnableConfiguration();
        this.testRunState = TestRunState.Stopping;

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Label(component), Is.EqualTo(StoppingLabel));
            Assert.That(IsDisabled(component), Is.True);
        }
    }

    [Test]
    public void Render__WhenNoTestIsSelected__ThenShouldNotLetRunStart()
    {
        // Given:
        this.configuration = ConfigurationSaidToBeRunnable();
        this.testSelection = new TestDiscoveryState();

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(IsDisabled(component), Is.True);
            Assert.That(Reason(component), Does.Contain("No tests selected"));
        }
    }

    private static TestConfigurationState ConfigurationSaidToBeRunnable()
        => new() { HasErrors = false };

    private static string Label(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).TextContent.Trim();

    private static bool IsDisabled(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).HasAttribute("disabled");

    private static string? Reason(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).GetAttribute("title");

    private void GivenARunnableConfiguration()
    {
        this.configuration = ConfigurationSaidToBeRunnable();
        this.GivenSelectedTestCase();
    }

    private void GivenSelectedTestCase()
        => this.testSelection = new TestDiscoveryState([GivenTestCasePath]);
}