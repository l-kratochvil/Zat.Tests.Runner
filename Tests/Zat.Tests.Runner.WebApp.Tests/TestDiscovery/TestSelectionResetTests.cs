namespace Zat.Tests.Runner.WebApp.Tests.TestDiscovery;

using Fluxor;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Features.TestDiscovery.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestSelectionResetTests
{
    private readonly Mock<ITestTreeStore> testTreeStoreMock = new();
    private readonly Mock<IDispatcher> dispatcherMock = new();
    private readonly TestSelectionReset unit;

    public TestSelectionResetTests()
    {
        this.unit = new TestSelectionReset(this.testTreeStoreMock.Object, this.dispatcherMock.Object);
        this.unit.Initialize();
    }

    [Test]
    public void OnTestTreeChanged__WhenExecutionPathsChanged__ThenShouldClearTestSelection()
    {
        // When:
        this.RaiseTestTreeChanged(executionPathsChanged: true);

        // Then:
        this.dispatcherMock.Verify(
            dispatcher => dispatcher.Dispatch(It.Is<SelectionChangedAction>(
                action => action.NewSelectedExecutionPaths != null
                          && action.NewSelectedExecutionPaths.Value.Count == 0)),
            Times.Once);
    }

    [Test]
    public void OnTestTreeChanged__WhenExecutionPathsDidNotChange__ThenShouldKeepTestSelection()
    {
        // When:
        this.RaiseTestTreeChanged(executionPathsChanged: false);

        // Then:
        this.dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public void OnTestTreeChanged__WhenCircuitIsGone__ThenShouldDoNothing()
    {
        // Given:
        this.unit.Dispose();

        // When:
        this.RaiseTestTreeChanged(executionPathsChanged: true);

        // Then:
        this.dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(It.IsAny<object>()), Times.Never);
    }

    private void RaiseTestTreeChanged(bool executionPathsChanged)
        => this.testTreeStoreMock.Raise(
            store => store.Changed += null,
            new TestTreeChange([], executionPathsChanged));
}