using System.Net;
using System.Net.Mail;

namespace TeachCodIT.Services
{
    public class EmailService   
    {
        private readonly IConfiguration _config;


        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient
            {
                Host = _config["SmtpSettings:Host"],
                Port = int.Parse(_config["SmtpSettings:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["SmtpSettings:Email"],
                    _config["SmtpSettings:Password"])
            };

            var message = new MailMessage(
                _config["SmtpSettings:Email"],
                toEmail,
                subject,
                body
            );

            await smtp.SendMailAsync(message);
        }
    }
}