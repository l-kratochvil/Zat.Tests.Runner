namespace Zat.Tests.Runner.Common.Net.Services;

using CommunityToolkit.Mvvm.Messaging;

public class Messanger : IMessenger
{
    private readonly WeakReferenceMessenger innerMessenger = new();

    /// <inheritdoc/>
    public void Cleanup()
        => this.innerMessenger.Cleanup();

    /// <inheritdoc/>
    public bool IsRegistered<TMessage, TToken>(object recipient, TToken token)
        where TMessage : class
        where TToken : IEquatable<TToken>
        => this.innerMessenger.IsRegistered<TMessage, TToken>(recipient, token);

    /// <inheritdoc/>
    public void Register<TRecipient, TMessage, TToken>(TRecipient recipient, TToken token, MessageHandler<TRecipient, TMessage> handler)
        where TRecipient : class
        where TMessage : class
        where TToken : IEquatable<TToken>
        => this.innerMessenger.Register(recipient, token, handler);

    /// <inheritdoc/>
    public void Reset()
        => this.innerMessenger.Reset();

    /// <inheritdoc/>
    public TMessage Send<TMessage, TToken>(TMessage message, TToken token)
        where TMessage : class
        where TToken : IEquatable<TToken>
        => this.innerMessenger.Send(message, token);

    public TMessage Send<TMessage>(TMessage message)
        where TMessage : class
        => this.innerMessenger.Send(message);

    /// <inheritdoc/>
    public void Unregister<TMessage, TToken>(object recipient, TToken token)
        where TMessage : class
        where TToken : IEquatable<TToken>
        => this.innerMessenger.Unregister<TMessage, TToken>(recipient, token);

    /// <inheritdoc/>
    public void UnregisterAll(object recipient)
        => this.innerMessenger.UnregisterAll(recipient);

    /// <inheritdoc/>
    public void UnregisterAll<TToken>(object recipient, TToken token)
        where TToken : IEquatable<TToken>
        => this.innerMessenger.UnregisterAll(recipient, token);
 }