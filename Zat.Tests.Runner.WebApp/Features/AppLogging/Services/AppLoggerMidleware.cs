namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Services;

using CommunityToolkit.Mvvm.Messaging;
using Fluxor;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

/// <summary>
/// Represents a middleware component for handling logging messages from global logger and dispatching them to the scoped application's state store.
/// </summary>
public sealed class AppLoggerMidleware
    : IRecipient<DataChangedAction>,
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

        globalMessanger.Register(this);
    }

    /// <inheritdoc/>
    public void Receive(DataChangedAction message)
        => this.dispatcher.Dispatch(message);

    /// <inheritdoc/>
    public void Dispose()
        => this.globalMessanger.UnregisterAll(this);
}