using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Communication.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly string port = configuration["Mailbox:Port"]!;
        private readonly string host = configuration["Mailbox:Host"]!;
        private readonly string username = configuration["Mailbox:Username"]!;
        private readonly string password = configuration["Mailbox:Password"]!;
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Send email
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Family App", username));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(host, int.Parse(port), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
