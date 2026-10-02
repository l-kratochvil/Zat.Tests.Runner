namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Services;

using CommunityToolkit.Mvvm.Messaging;
using DevKit.Core.Interfaces;
using Fluxor;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

/// <summary>
/// Represents a middleware component for handling logging messages from global logger and dispatching them to the scoped application's state store.
/// </summary>
/// <remarks>
/// Nothing depends on it, so it starts listening when its circuit initialises it.
/// </remarks>
public sealed class AppLoggerMidleware
    : IRecipient<DataChangedAction>,
      IInitializable,
      IDisposable
{
    private readonly GlobalWeakReferenceMessanger globalMessanger;
    private readonly IDispatcher dispatcher;

    public AppLoggerMidleware(
        GlobalWeakReferenceMessanger globalMessanger,
        IDispatcher dispatcher)
    {
        this.globalMessanger = globalMessanger;
        this.dispatcher = dispatcher;
    }

    /// <summary>
    /// Starts forwarding the global log messages to the circuit's store.
    /// </summary>
    public void Initialize()
        => this.globalMessanger.Register(this);

    /// <inheritdoc/>
    public void Receive(DataChangedAction message)
        => this.dispatcher.Dispatch(message);

    /// <inheritdoc/>
    public void Dispose()
        => this.globalMessanger.UnregisterAll(this);
}