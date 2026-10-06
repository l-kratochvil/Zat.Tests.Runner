namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using DevKit.Core.Extensions.Types;
using Fluxor;

public sealed class StateBinder<TViewModel, TState>(
    TViewModel viewmodel,
    IState<TState> state)
    : IDisposable
    where TViewModel : ViewModelBase
{
    private readonly ConcurrentBag<IDisposable> bindings = [];

    /// <summary>
    /// Binds a property of the view model to a selected value from the state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="selectValue"></param>
    /// <param name="property"></param>
    /// <param name="writeValue"></param>
    /// <returns></returns>
    public StateBinding<TValue> Bind<TValue>(
        Func<TState, TValue> selectValue,
        Expression<Func<TViewModel, TValue>> property,
        Action<TValue>? writeValue = null)
    {
        var binding = viewmodel.BindToState(state, selectValue, property, writeValue);
        this.bindings.Add(binding);
        return binding;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.bindings.DisposeAll();
        this.bindings.Clear();
    }
}