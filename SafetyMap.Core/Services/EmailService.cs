using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SafetyMap.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace SafetyMap.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var host = _configuration["Mailtrap:Host"];
            var userName = _configuration["Mailtrap:Username"];
            var password = _configuration["Mailtrap:Password"];
            var fromEmailAddress = _configuration["Mailtrap:FromEmail"] ?? "noreply@safetymap.com";
            var fromName = _configuration["Mailtrap:FromName"] ?? "SafetyMap Alerts";
            int.TryParse(_configuration["Mailtrap:Port"], out int port);
            
            if (port == 0) port = 465;

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email SMTP configuration is missing. Host: {Host}, User: {User}, HasPassword: {HasPassword}. Email will not be sent.", 
                    host ?? "null", userName ?? "null", !string.IsNullOrEmpty(password));
                return;
            }

            _logger.LogInformation("Attempting to send email to {ToEmail} via {Host}:{Port} using SSL...", toEmail, host, port);

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmailAddress));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // SecureSocketOptions.Auto handles both implicit SSL (465) and STARTTLS (587)
                await client.ConnectAsync(host, port, SecureSocketOptions.Auto);

                await client.AuthenticateAsync(userName, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email successfully sent to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to {ToEmail} via MailKit", toEmail);
            }
        }
    }
}
