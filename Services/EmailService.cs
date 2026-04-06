using System;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using MimeKit;

namespace EasySheba.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string ReceiverEmail { get; set; }
    }

    public class ContactMessage
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public interface IEmailService
    {
        // Returns (success, errorMessage)
        Task<(bool Success, string ErrorMessage)> SendContactEmailAsync(ContactMessage message);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _env;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IWebHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _env = env;
        }

        public async Task<(bool Success, string ErrorMessage)> SendContactEmailAsync(ContactMessage contactMessage)
        {
            try
            {
                // Create email
                var email = new MimeMessage();

                // From (use configured sender or fallback)
                var senderEmail = !string.IsNullOrEmpty(_emailSettings.SenderEmail) ? _emailSettings.SenderEmail : "no-reply@easysheba.com";
                email.From.Add(new MailboxAddress("EasySheba Contact", senderEmail));

                // To: configured receiver (ensure all messages go to professionalhani7@gmail.com)
                var receiver = !string.IsNullOrEmpty(_emailSettings.ReceiverEmail) ? _emailSettings.ReceiverEmail : "professionalhani7@gmail.com";
                email.To.Add(new MailboxAddress("Admin", receiver));

                // Reply-To: Patient's email (so admin can reply)
                if (!string.IsNullOrEmpty(contactMessage.Email))
                {
                    email.ReplyTo.Add(new MailboxAddress(contactMessage.Name ?? "", contactMessage.Email));
                }

                // Subject
                email.Subject = !string.IsNullOrEmpty(contactMessage.Subject)
                    ? contactMessage.Subject
                    : $"New Contact Form Message from {contactMessage.Name}";

                // Email body (plain text)
                email.Body = new TextPart("plain")
                {
                    Text = $@"New Contact Form Message
------------------------
Name: {contactMessage.Name}
Email: {contactMessage.Email}
Subject: {contactMessage.Subject}

Message:
{contactMessage.Message}

------------------------
This email was sent from EasySheba Contact Form."
                };

                using var smtp = new SmtpClient();

                var smtpServer = string.IsNullOrEmpty(_emailSettings.SmtpServer) ? "smtp.gmail.com" : _emailSettings.SmtpServer;
                var smtpPort = _emailSettings.SmtpPort > 0 ? _emailSettings.SmtpPort : 587;

                // Try STARTTLS on port (usually 587)
                try
                {
                    await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                }
                catch (Exception exStart)
                {
                    _logger.LogWarning(exStart, "STARTTLS connection failed, will try SSL on 465");
                    // Try SSL on 465 as fallback
                    try
                    {
                        await smtp.ConnectAsync(smtpServer, 465, SecureSocketOptions.SslOnConnect);
                    }
                    catch (Exception exSsl)
                    {
                        _logger.LogError(exSsl, "Both STARTTLS and SSL connection attempts to SMTP server failed");
                        // Save email to local file for inspection
                        TrySaveEmailToFile(email, "connect_failure");
                        return (false, "Could not connect to SMTP server: " + exSsl.Message);
                    }
                }

                // If credentials provided, authenticate
                if (!string.IsNullOrEmpty(_emailSettings.SenderEmail) && !string.IsNullOrEmpty(_emailSettings.SenderPassword))
                {
                    try
                    {
                        await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                    }
                    catch (Exception exAuth)
                    {
                        _logger.LogError(exAuth, "SMTP authentication failed for {Sender}", _emailSettings.SenderEmail);
                        TrySaveEmailToFile(email, "auth_failure");
                        return (false, "SMTP authentication failed: " + exAuth.Message);
                    }
                }

                try
                {
                    await smtp.SendAsync(email);
                }
                catch (Exception exSend)
                {
                    _logger.LogError(exSend, "Failed to send email message");
                    TrySaveEmailToFile(email, "send_failure");
                    return (false, "Failed to send email: " + exSend.Message);
                }

                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Contact email sent to {Receiver}", receiver);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending contact email");
                TrySaveEmailToFile(null, "unexpected_failure", ex.Message);
                return (false, "Unexpected error: " + ex.Message);
            }
        }

        private void TrySaveEmailToFile(MimeMessage email, string reason, string extra = null)
        {
            try
            {
                var emailsDir = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "emails");
                if (!Directory.Exists(emailsDir)) Directory.CreateDirectory(emailsDir);

                var fileName = $"email_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{reason}.eml";
                var filePath = Path.Combine(emailsDir, fileName);

                if (email != null)
                {
                    using var stream = System.IO.File.Create(filePath);
                    email.WriteTo(stream);
                }
                else
                {
                    var text = $"Email could not be sent. Reason: {reason}\nExtra: {extra}";
                    System.IO.File.WriteAllText(filePath + ".txt", text);
                }

                _logger.LogInformation("Saved unsent email to {Path}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save unsent email to file");
            }
        }
    }
}