namespace Zat.Tests.Runner.WebApp.Components;

using System.ComponentModel;

using Fluxor.Blazor.Web.Components;

using Microsoft.AspNetCore.Components;

using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// A component that redraws itself whenever the view model it is drawn from reports a change.
/// </summary>
/// <typeparam name="TDataContext">The view model the component is drawn from.</typeparam>
public abstract class MvvmComponentBase<TDataContext> : FluxorComponent
    where TDataContext : class, INotifyPropertyChanged, INotifyDataInfo
{
    private int renderPending;

    private volatile bool disposed;

    /// <summary>
    /// Gets or sets the view model this component is drawn from, found on first use.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The component neither makes a view model nor stands under a
    /// <see cref="Primitives.DataContext{TViewModel}"/> that cascades one.
    /// </exception>
    protected virtual TDataContext ViewModel
    {
        get => field ??= this.DataContext ?? throw new InvalidOperationException(
            $"No data context was provided to {this.GetType().Name}. Place the component inside " +
            $"{nameof(Primitives.DataContext<>)} of {typeof(TDataContext).Name}.");
        set;
    }

    /// <summary>
    /// Gets or sets the view model cascaded to this component, if there is one.
    /// </summary>
    /// <remarks>
    /// Read through <see cref="ViewModel"/> rather than directly, so that a component drawn from a
    /// cascaded view model and one drawn from a view model of its own are written the same way.
    /// </remarks>
    [CascadingParameter]
    private TDataContext? DataContext { get; set; }

    /// <summary>
    /// Says whether a change of the named property is one this component is drawn from.
    /// </summary>
    /// <remarks>
    /// Answered on the thread that raised the change, which need not be this component's circuit,
    /// so the answer may be read from the view model but nothing may be drawn from here.
    /// </remarks>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the component should re-render; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldRerenderOn(string? propertyName)
        => true;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
        this.ViewModel.DataChanged += this.OnViewModelDataChanged;
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        // The renderer disposes of a component that is IAsyncDisposable — which FluxorComponent is
        // — through that alone, so everything this class owns is let go of here. An IDisposable of
        // its own would never be called.
        if (disposing)
        {
            this.disposed = true;
            this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            this.ViewModel.DataChanged -= this.OnViewModelDataChanged;
        }

        await base.DisposeAsyncCore(disposing);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (this.disposed || !this.ShouldRerenderOn(e.PropertyName))
        {
            return;
        }

        // A view model fed by a store is told of a change on the thread of whoever made it, which
        // is somebody else's circuit as often as not, so several threads can arrive here at once.
        // Exchanging the flag rather than reading it and then setting it is what makes one render
        // out of a burst a guarantee instead of a likelihood.
        if (Interlocked.Exchange(ref this.renderPending, 1) is 1)
        {
            return;
        }

        try
        {
            _ = this.InvokeAsync(() =>
            {
                // Cleared before drawing, so that a change raised while the component renders asks
                // for the next render instead of being swallowed by the one under way.
                Interlocked.Exchange(ref this.renderPending, 0);

                if (!this.disposed)
                {
                    this.StateHasChanged();
                }
            });
        }
        catch (ObjectDisposedException)
        {
            // The circuit went away between the change and this call. Letting the exception out
            // would stop the change reaching the listeners after this one.
            Interlocked.Exchange(ref this.renderPending, 0);
        }
    }

    private void OnViewModelDataChanged(object? sender, EventArgs e)
    {
        if (this.disposed)
        {
            return;
        }

        // A view model fed by a store is told of a change on the thread of whoever made it, which
        // is somebody else's circuit as often as not, so several threads can arrive here at once.
        // Exchanging the flag rather than reading it and then setting it is what makes one render
        // out of a burst a guarantee instead of a likelihood.
        if (Interlocked.Exchange(ref this.renderPending, 1) is 1)
        {
            return;
        }

        try
        {
            _ = this.InvokeAsync(() =>
            {
                // Cleared before drawing, so that a change raised while the component renders asks
                // for the next render instead of being swallowed by the one under way.
                Interlocked.Exchange(ref this.renderPending, 0);

                if (!this.disposed)
                {
                    this.StateHasChanged();
                }
            });
        }
        catch (ObjectDisposedException)
        {
            // The circuit went away between the change and this call. Letting the exception out
            // would stop the change reaching the listeners after this one.
            Interlocked.Exchange(ref this.renderPending, 0);
        }
    }
}