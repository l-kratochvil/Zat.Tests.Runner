namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using Fluxor;

using Moq;

using Zat.Tests.Runner.WebApp.Features.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

/// <summary>
/// The test configuration state as Fluxor would keep it: what is dispatched goes through the real
/// reducers, and a state that came out different is announced.
/// </summary>
/// <remarks>
/// <see cref="Dispatcher"/> stays a mock, so what was dispatched can still be verified.
/// </remarks>
internal sealed class TestConfigurationStoreFake : IState<TestConfigurationState>
{
    public TestConfigurationStoreFake()
    {
        this.Dispatcher = new Mock<IDispatcher>();
        this.Dispatcher
            .Setup(dispatcher => dispatcher.Dispatch(It.IsAny<object>()))
            .Callback<object>(this.Reduce);
    }

    /// <inheritdoc/>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Gets the dispatcher that hands what is dispatched over to the reducers.
    /// </summary>
    public Mock<IDispatcher> Dispatcher { get; }

    /// <summary>
    /// Gets or sets the state as it is now; setting it does not announce anything, so it is how a
    /// test says what the state was to begin with.
    /// </summary>
    public TestConfigurationState Value { get; set; } = new();

    /// <summary>
    /// Changes the state the way something other than the view model would, announcing it.
    /// </summary>
    /// <param name="next">The state to change to.</param>
    public void ChangeTo(TestConfigurationState next)
    {
        this.Value = next;
        this.StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Reduce(object action)
    {
        var next = action switch
        {
            DataChangedAction dataChanged => Reducers.OnDataChanged(this.Value, dataChanged),
            StatusChangedAction statusChanged => Reducers.OnStatusChanged(this.Value, statusChanged),
            _ => this.Value,
        };

        if (!ReferenceEquals(next, this.Value))
        {
            this.ChangeTo(next);
        }
    }
}