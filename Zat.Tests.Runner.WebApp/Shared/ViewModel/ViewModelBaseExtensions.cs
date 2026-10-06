namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using System.Linq.Expressions;

using Fluxor;

/// <summary>
/// Ties properties of a <see cref="ViewModelBase"/> to values held in a Fluxor state.
/// </summary>
public static class ViewModelBaseExtensions
{
    extension<TViewModel>(TViewModel self)
        where TViewModel : ViewModelBase
    {
        /// <summary>
        /// Draws <paramref name="property"/> from the value <paramref name="selectValue"/> picks out of
        /// <paramref name="state"/>, announcing a change of the property whenever the value changes.
        /// </summary>
        /// <typeparam name="TState">The state the value is held in.</typeparam>
        /// <typeparam name="TValue">The type of the value and of the property.</typeparam>
        /// <param name="state">The state the value is read from.</param>
        /// <param name="selectValue">Picks the value out of <paramref name="state"/>.</param>
        /// <param name="property">The property of <paramref name="viewModel"/> drawn from the value.</param>
        /// <param name="writeValue">
        /// Hands a new value over to <paramref name="state"/>, typically by dispatching an action, or
        /// <see langword="null"/> where the value is only shown.
        /// </param>
        /// <returns>The <see cref="StateBinding{TValue}"/> the property reads from; disposing of it ends the binding.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="property"/> is not a property of <typeparamref name="TViewModel"/>.
        /// </exception>
        public StateBinding<TValue> BindToState<TState, TValue>(
            IState<TState> state,
            Func<TState, TValue> selectValue,
            Expression<Func<TViewModel, TValue>> property,
            Action<TValue>? writeValue = null)
        {
            var propertyName = Helpers.GetExpressionPropertyName(property);
            var lastValue = selectValue(state.Value);

            void OnStateChanged(object? sender, EventArgs e)
            {
                var newValue = selectValue(state.Value);

                // A state changes as a whole, so a change of any other value in it is no news here.
                if (EqualityComparer<TValue>.Default.Equals(lastValue, newValue))
                {
                    return;
                }

                lastValue = newValue;
                self.NotifyPropertyChanged(propertyName);
            }

            TValue ReadValue()
                => selectValue(state.Value);

            state.StateChanged += OnStateChanged;

            return new StateBinding<TValue>(
                readValue: ReadValue,
                writeValue: writeValue is { } write
                    ? value =>
                    {
                        // A control writes back what it shows on every edit, which is mostly what is
                        // held already.
                        if (!EqualityComparer<TValue>.Default.Equals(value, ReadValue()))
                        {
                            write(value);
                        }
                    }
                    : null,
                unsubscribe: () => state.StateChanged -= OnStateChanged);
        }
    }
}