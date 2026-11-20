namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Sends emails; the server, its credentials and the whole message are given on every send, so one sender
/// serves any email and any server.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends <paramref name="message"/> through the server given by <paramref name="config"/>.
    /// </summary>
    /// <param name="config">The server to send through and its credentials.</param>
    /// <param name="message">The sent message.</param>
    /// <exception cref="Exception">The email could not be sent.</exception>
    void Send(SendConfig config, EmailMessage message);

    /// <summary>
    /// The server an email is sent through.
    /// </summary>
    /// <param name="Server">The server as <c>host[:port]</c>; the port defaults to 25.</param>
    /// <param name="AuthUsername">The username to authenticate with.</param>
    /// <param name="AuthPassword">The password to authenticate with.</param>
    public record SendConfig(
        string Server,
        string AuthUsername,
        string AuthPassword);

    /// <summary>
    /// A plain-text email.
    /// </summary>
    /// <param name="From">The sender address.</param>
    /// <param name="To">The recipient addresses.</param>
    /// <param name="Subject">The subject.</param>
    /// <param name="Body">The plain-text body.</param>
    public record EmailMessage(
        string From,
        IReadOnlyList<string> To,
        string Subject,
        string Body);
}