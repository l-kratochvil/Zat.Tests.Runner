namespace Zat.Tests.Runner.WebApp.Tests.TestExecution;

using Fluxor;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Features.TestExecution.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestExecution;

[TestFixture]
public class TestRunnerEngineStateHandlerTests
{
    [Test]
    public void OnTestRunStateChanged__WhenTestRunStarts__ThenShouldAnnounceIt()
    {
        // Given:
        var testRunnerEngineMock = new Mock<ITestRunnerEngine>();
        var dispatcherMock = new Mock<IDispatcher>();
        using var unit = CreateUnit(testRunnerEngineMock.Object, dispatcherMock.Object);

        // When:
        RaiseStateChanged(testRunnerEngineMock, TestRunState.Running);

        // Then:
        dispatcherMock.Verify(
            dispatcher => dispatcher.Dispatch(It.IsAny<TestRunStartedAction>()),
            Times.Once);
    }

    [TestCase(TestRunState.Stopping)]
    [TestCase(TestRunState.Stopped)]
    public void OnTestRunStateChanged__WhenTestRunDoesNotStart__ThenShouldDoNothing(TestRunState givenState)
    {
        // Given:
        var testRunnerEngineMock = new Mock<ITestRunnerEngine>();
        var dispatcherMock = new Mock<IDispatcher>();
        using var unit = CreateUnit(testRunnerEngineMock.Object, dispatcherMock.Object);

        // When:
        RaiseStateChanged(testRunnerEngineMock, givenState);

        // Then:
        dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public void OnTestRunStateChanged__WhenCircuitIsGone__ThenShouldDoNothing()
    {
        // Given:
        var testRunnerEngineMock = new Mock<ITestRunnerEngine>();
        var dispatcherMock = new Mock<IDispatcher>();
        using var unit = CreateUnit(testRunnerEngineMock.Object, dispatcherMock.Object);
        unit.Dispose();

        // When:
        RaiseStateChanged(testRunnerEngineMock, TestRunState.Running);

        // Then:
        dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(It.IsAny<object>()), Times.Never);
    }

    private static TestRunnerEngineStateHandler CreateUnit(
        ITestRunnerEngine? testRunnerEngine = null,
        IDispatcher? dispatcher = null)
    {
        var unit = new TestRunnerEngineStateHandler(
            testRunnerEngine ?? Mock.Of<ITestRunnerEngine>(),
            dispatcher ?? Mock.Of<IDispatcher>());
        unit.Initialize();
        return unit;
    }

    private static void RaiseStateChanged(Mock<ITestRunnerEngine> testRunnerEngineMock, TestRunState state)
        => testRunnerEngineMock.Raise(engine => engine.StateChanged += null, state);
}