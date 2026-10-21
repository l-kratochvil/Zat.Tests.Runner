namespace Zat.Tests.Runner.Common.Net.Services;

using System.Net;
using System.Net.Mail;

using static Zat.Tests.Runner.Common.Net.Services.IEmailSender;

/// <summary>
/// Sends emails over SMTP, always upgrading the connection by STARTTLS.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private const int DefaultPort = 25;

    /// <inheritdoc />
    public void Send(SendConfig config, EmailMessage message)
    {
        var (host, port) = ParseServer(config.Server);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(config.AuthUsername, config.AuthPassword),
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(message.From),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = false,
        };

        foreach (var recipient in message.To)
        {
            mailMessage.To.Add(recipient);
        }

        client.Send(mailMessage);
    }

    private static (string Host, int Port) ParseServer(string server)
    {
        var colonIndex = server.LastIndexOf(':');

        return colonIndex < 0
            ? (server, DefaultPort)
            : (server[..colonIndex], int.Parse(server[(colonIndex + 1)..]));
    }
}