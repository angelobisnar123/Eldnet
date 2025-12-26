using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace WebApplication1_Test1.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; } = false;
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplayName { get; set; }
        public string AdminEmail { get; set; }
    }

    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public EmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);

            var from = new MailAddress(_options.FromEmail, _options.FromDisplayName);
            var to = new MailAddress(email);

            using var msg = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            // synchronous send (simple). For production prefer MailKit + background queue.
            client.Send(msg);

            return Task.CompletedTask;
        }
    }
}