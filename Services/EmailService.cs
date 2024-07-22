using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NuGet.Configuration;

namespace ConnectingDatabase.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings"); // retrieves the settings from your appsettings.json

            var emailMessage = new MimeMessage(); //initializes a new email message
            emailMessage.From.Add(new MailboxAddress("StudentHub", emailSettings["FromEmail"]));
            emailMessage.To.Add(new MailboxAddress("Recipient", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), SecureSocketOptions.StartTls);
                client.Authenticate(emailSettings["SmtpUser"], emailSettings["SmtpPass"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
