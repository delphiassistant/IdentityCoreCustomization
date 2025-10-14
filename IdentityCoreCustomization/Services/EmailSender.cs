using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit;

namespace IdentityCoreCustomization.Services
{
    public class EmailOptions
    {
        public string FromName { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public SmtpOptions Smtp { get; set; } = new();
    }

    public class SmtpOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string SecureSocket { get; set; } = "StartTls"; // None | SslOnConnect | StartTls
        public bool SkipCertValidation { get; set; } = false;
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _options;

        public EmailSender(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        private static string StripHtmlToText(string html) =>
            System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);

            public async Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(MimeKit.MailboxAddress.Parse(email));
            message.Subject = subject;

            var bodyBuilder = new MimeKit.BodyBuilder
                {
                    HtmlBody = htmlMessage,
                    TextBody = StripHtmlToText(htmlMessage)
                };
            message.Body = bodyBuilder.ToMessageBody();

                var secure = _options.Smtp.SecureSocket switch
                {
                "None" => MailKit.Security.SecureSocketOptions.None,
                "SslOnConnect" => MailKit.Security.SecureSocketOptions.SslOnConnect,
                _ => MailKit.Security.SecureSocketOptions.StartTls
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();

                if (_options.Smtp.SkipCertValidation)
                    client.ServerCertificateValidationCallback = (_, _, _, _) => true;

                await client.ConnectAsync(_options.Smtp.Host, _options.Smtp.Port, secure);

            if (!string.IsNullOrWhiteSpace(_options.Smtp.UserName))
                await client.AuthenticateAsync(_options.Smtp.UserName, _options.Smtp.Password);

            await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
    }
}
