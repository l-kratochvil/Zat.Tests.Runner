public interface IStore<TState>
    where TState : class
{
    /// <summary>
    /// Raised after the state has changed, on the thread of the caller that changed it.
    /// </summary>
    event Action<TState>? Changed;

    /// <summary>
    /// Gets the current store state.
    /// </summary>
    TState Current { get; }
}
