using System.Net;
using System.Net.Mail;
using Application.Common;
using Application.Common.Settings;
using Application.Interfaces.Interface;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class SmtpService : ISmtpService
{
    private readonly SmtpSettings _settings;

    public SmtpService(IOptions<SmtpSettings> smtpOptions)
    {
        _settings = smtpOptions.Value;
    }

    public async Task<Result> SendEmailAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            return Result.Failure("Email recipient is required");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            return Result.Failure("Email subject is required");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure("Email body is required");
        }

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            return Result.Failure("SMTP host is not configured");
        }

        if (_settings.Port <= 0)
        {
            return Result.Failure("SMTP port is not configured correctly");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            return Result.Failure("Sender email is not configured");
        }

        try
        {
            using SmtpClient smtpClient = new SmtpClient(_settings.Host, _settings.Port);
            smtpClient.EnableSsl = _settings.EnableSsl;

            if (!string.IsNullOrWhiteSpace(_settings.UserName))
            {
                smtpClient.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
            }

            using MailMessage message = new MailMessage(_settings.FromEmail, to.Trim(), subject.Trim(), body.Trim());
            message.IsBodyHtml = false;

            await smtpClient.SendMailAsync(message);

            return Result.Success();
        }
        catch
        {
            return Result.Failure("Email sending failed");
        }
    }
}
