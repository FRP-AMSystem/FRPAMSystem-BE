using FRPAMSystem.BusinessTier.Configuration;
using FRPAMSystem.BusinessTier.Payload.Email;
using FRPAMSystem.BusinessTier.Services.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(SendEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                throw new Exception("Recipient email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new Exception("Email subject is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                throw new Exception("Email body is required.");
            }

            if (!_settings.Enabled)
            {
                _logger.LogInformation(
                    "Email sending is disabled. Skipped email to {ToEmail} with subject '{Subject}'.",
                    request.ToEmail,
                    request.Subject);
                return;
            }

            ValidateSmtpSettings();

            var message = BuildMessage(request);

            using var client = new SmtpClient();
            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureSocketOptions);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "Email sent to {ToEmail} with subject '{Subject}'.",
                request.ToEmail,
                request.Subject);
        }

        private void ValidateSmtpSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                throw new Exception("SMTP host is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                throw new Exception("Sender email is not configured.");
            }
        }

        private MimeMessage BuildMessage(SendEmailRequest request)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(request.ToName ?? request.ToEmail, request.ToEmail));
            message.Subject = request.Subject.Trim();
            message.Body = new TextPart("plain")
            {
                Text = request.Body.Trim()
            };

            return message;
        }
    }
}
