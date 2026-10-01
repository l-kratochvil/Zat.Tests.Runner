namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using System.Linq.Expressions;
using System.Reflection;

using Fluxor;

/// <summary>
/// Ties properties of a <see cref="ViewModelBase"/> to values held in a Fluxor state.
/// </summary>
public static class ViewModelBindingExtensions
{
    /// <summary>
    /// Draws <paramref name="property"/> from the value <paramref name="selectValue"/> picks out of
    /// <paramref name="state"/>, announcing a change of the property whenever the value changes.
    /// </summary>
    /// <typeparam name="TViewModel">The view model the property belongs to.</typeparam>
    /// <typeparam name="TState">The state the value is held in.</typeparam>
    /// <typeparam name="TValue">The type of the value and of the property.</typeparam>
    /// <param name="viewModel">The view model to announce the change on.</param>
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
    public static StateBinding<TValue> BindToState<TViewModel, TState, TValue>(
        this TViewModel viewModel,
        IState<TState> state,
        Func<TState, TValue> selectValue,
        Expression<Func<TViewModel, TValue>> property,
        Action<TValue>? writeValue = null)
        where TViewModel : ViewModelBase
    {
        var propertyName = GetPropertyName(property);
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
            viewModel.NotifyPropertyChanged(propertyName);
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

    private static string GetPropertyName<TViewModel, TValue>(
        Expression<Func<TViewModel, TValue>> property)
        => property.Body is MemberExpression { Member: PropertyInfo info, Expression: ParameterExpression }
            ? info.Name
            : throw new ArgumentException(
                $"Expression '{property}' must name a property of {typeof(TViewModel).Name}, e.g. vm => vm.Value.",
                nameof(property));
}