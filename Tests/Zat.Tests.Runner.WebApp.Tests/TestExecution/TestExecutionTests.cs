namespace Zat.Tests.Runner.WebApp.Tests.TestExecution;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

using TestExecutionComponent = Zat.Tests.Runner.WebApp.Features.TestExecution.Components.TestExecution;

/// <summary>
/// What the button offers and what stops it, which is all it does.
/// </summary>
/// <remarks>
/// Whether a configuration can be run with is not asked here: the button only reads the answer the
/// configurator put into the state, and the rules behind it are exercised in
/// <see cref="TestConfiguration.TestConfigurationValidatorTests"/>.
/// </remarks>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestExecutionTests : Bunit.TestContext
{
    private const string ButtonSelector = "button";

    private const string StartLabel = "Start";
    private const string StopLabel = "Stop";

    private TestConfigurationState configuration = new();
    private TestDiscoveryState testSelection = new([]);

    [SetUp]
    public void SetUp()
    {
        var configurationState = new Mock<IState<TestConfigurationState>>();
        configurationState.SetupGet(state => state.Value).Returns(() => this.configuration);

        var testDiscoveryStore = new Mock<ITestDiscoveryStore>();
        testDiscoveryStore.SetupGet(store => store.Current).Returns(() => this.testSelection);

        this.Services.AddSingleton(configurationState.Object);
        this.Services.AddSingleton(testDiscoveryStore.Object);

        // The button reaches for both of these as soon as it is drawn — the runner because a click
        // would need it, the subscriber because the button is a Fluxor component — so neither can
        // be left out of a fixture that only asks what stops a run.
        this.Services.AddSingleton(new Mock<INUnitTestRunnerProxy>().Object);
        this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenNothingHasBeenClicked__ThenShouldOfferToStartTheRun()
    {
        // Given:
        this.GivenARunnableConfiguration();

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.That(Label(component), Is.EqualTo(StartLabel));
    }

    [TestCase(1, StopLabel)]
    [TestCase(2, StartLabel)]
    public void OnToggle__WhenTheButtonIsClicked__ThenShouldOfferTheOppositeAction(
        int givenClicks, string expectedLabel)
    {
        // Given:
        this.GivenARunnableConfiguration();

        var component =
            this.RenderComponent<TestExecutionComponent>();

        // When:
        for (var click = 0; click < givenClicks; click++)
        {
            component.Find(ButtonSelector).Click();
        }

        // Then:
        Assert.That(Label(component), Is.EqualTo(expectedLabel));
    }

    [Test]
    public void Render__WhenNoTestIsSelected__ThenShouldNotLetTheRunStart()
    {
        // Given:
        // A run of nothing is not a run, and the reason sits on the button because that is what the
        // tester is looking at rather than the explorer beside it.
        this.configuration = ConfigurationSaidToBeRunnable();
        this.testSelection = new TestDiscoveryState([]);

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(IsDisabled(component), Is.True);
            Assert.That(Reason(component), Does.Contain("Select the tests"));
        });
    }

    [Test]
    public void Render__WhenTheConfigurationCannotBeRunWith__ThenShouldNotLetTheRunStart()
    {
        // Given:
        // What is wrong with it is not said here: the configurator is beside the button and says it
        // field by field.
        this.configuration = new TestConfigurationState { HasErrors = true };
        this.GivenSelectedTestCase(TestType.ApplicationTest);

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(IsDisabled(component), Is.True);
            Assert.That(Reason(component), Does.Contain("Complete the test configuration"));
        });
    }

    [Test]
    public void Render__WhenTheSelectionAndTheConfigurationAreBothThere__ThenShouldLetTheRunStart()
    {
        // Given:
        this.GivenARunnableConfiguration();

        // When:
        var component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.That(IsDisabled(component), Is.False);
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
        this.GivenSelectedTestCase(TestType.ApplicationTest);
    }

    private void GivenSelectedTestCase(TestType testType)
        => this.testSelection = new TestDiscoveryState(
            [new TestCaseEntity(testType, id: "1", name: "Test", executionPath: "Suite.Fixture.Test")]);
}