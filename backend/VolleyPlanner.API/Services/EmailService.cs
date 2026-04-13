using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VolleyPlanner.API.Interfaces;
using VolleyPlanner.API.Models;

namespace VolleyPlanner.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _emailSettings.SenderName,
            _emailSettings.SenderEmail));

        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _emailSettings.SmtpHost,
            _emailSettings.SmtpPort,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _emailSettings.SmtpUsername,
            _emailSettings.SmtpPassword);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}