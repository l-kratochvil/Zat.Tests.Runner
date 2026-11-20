namespace Zat.Tests.Runner.WebApp.Shared.Stores;

/// <summary>
/// Applies the parts of an action that carry a change, so a reducer never has to say what to do
/// about a value that was left alone.
/// </summary>
/// <typeparam name="TState">State the changes are applied to.</typeparam>
public sealed class StateUpdater<TState>
{
    /// <summary>
    /// Starts a chain of changes over <paramref name="current"/>.
    /// </summary>
    /// <typeparam name="TValue">Value the change carries.</typeparam>
    /// <param name="current">State as it stands.</param>
    /// <param name="change">
    /// The change to apply, or <see langword="null"/> when the value was left alone.
    /// </param>
    /// <param name="update">Puts the changed value into the state.</param>
    /// <returns>
    /// The chain carrying the state with <paramref name="change"/> applied, to be ended with
    /// <see cref="UpdateScope{TState}.Complete"/>.
    /// </returns>
    public UpdateScope UpdateIfChanged<TValue>(
        TState current,
        ValueChange<TValue>? change,
        Func<TState, TValue, TState> update)
        => new UpdateScope(current).UpdateIfChanged(change, update);

    /// <summary>
    /// A state being taken through the changes an action carries, one value at a time.
    /// </summary>
    /// <typeparam name="TState">State the changes are applied to.</typeparam>
    /// <param name="current">State as the changes applied so far leave it.</param>
    public sealed class UpdateScope(TState current)
    {
        /// <summary>
        /// Applies <paramref name="change"/> to the state the chain carries.
        /// </summary>
        /// <typeparam name="TValue">Value the change carries.</typeparam>
        /// <param name="change">
        /// The change to apply, or <see langword="null"/> when the value was left alone.
        /// </param>
        /// <param name="update">Puts the changed value into the state.</param>
        /// <returns>The chain carrying the state with <paramref name="change"/> applied.</returns>
        public UpdateScope UpdateIfChanged<TValue>(
            ValueChange<TValue>? change,
            Func<TState, TValue, TState> update)
            => change is null ? this : new UpdateScope(update(current, change.Value));

        /// <summary>
        /// Ends the chain.
        /// </summary>
        /// <returns>The state every change of the chain has been applied to.</returns>
        public TState Complete()
            => current;
    }
}