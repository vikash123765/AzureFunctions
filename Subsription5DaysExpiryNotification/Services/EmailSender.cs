using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Text;

namespace Subscription5DaysExpiryNotification.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_configuration["SenderEmail"]);
            message.Sender.Name = _configuration["SenderName"];
            message.To.Add(MailboxAddress.Parse(email));
            message.From.Add(message.Sender);
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            using (var emailClient = new SmtpClient())
            {
                try
                {
                    await emailClient.ConnectAsync(_configuration["SmtpServer"], int.Parse(_configuration["SmtpPort"]), SecureSocketOptions.SslOnConnect);
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    await emailClient.AuthenticateAsync(_configuration["SmtpUsername"], _configuration["SmtpPassword"]);
                    await emailClient.SendAsync(message);
                    await emailClient.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }
    }
}
