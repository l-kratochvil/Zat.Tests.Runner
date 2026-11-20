namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// A value of a Fluxor state that a view model property is drawn from, and optionally written to.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <param name="readValue">Reads the value as the state holds it now.</param>
/// <param name="writeValue">
/// Hands a new value over to the state, or <see langword="null"/> where the value is only shown.
/// </param>
/// <param name="unsubscribe">Stops listening to the state.</param>
/// <remarks>
/// Hides the state the value is held in, so the view model names only what it shows.
/// Created through <see cref="ViewModelBindingExtensions.BindToState{TViewModel, TState, TValue}"/>.
/// </remarks>
public sealed class StateBinding<TValue>(
    Func<TValue> readValue,
    Action<TValue>? writeValue,
    Action unsubscribe)
    : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the value can only be read.
    /// </summary>
    public bool IsReadOnly
        => writeValue is null;

    /// <summary>
    /// Gets or sets the value as the state holds it.
    /// </summary>
    /// <remarks>
    /// A value written is not read back until the state has taken it over.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The value is written while <see cref="IsReadOnly"/> is <see langword="true"/>.
    /// </exception>
    public TValue Value
    {
        get => readValue();
        set
        {
            if (writeValue is null)
            {
                throw new InvalidOperationException(
                    $"The {nameof(StateBinding<>)} of {typeof(TValue).Name} is read-only.");
            }

            writeValue(value);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => unsubscribe();
}