using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ICWebAPI.Models;
using Microsoft.Extensions.Options;

namespace ICWebAPI.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSetting _emailSetting;

        public EmailSender(IOptions<EmailSetting> emailSetting)
        {
            _emailSetting = emailSetting.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var toEmail = string.IsNullOrEmpty(email) ? _emailSetting.ToEmail : email;

            var mail = new MailMessage()
            {
                From = new MailAddress(_emailSetting.UsernameEmail, "IC"),
                To = { new MailAddress(toEmail) },
                CC = { new MailAddress(_emailSetting.CcEmail) },
                Subject = "IdentityCore - " + subject,
                Body = message,
                IsBodyHtml = true,
                Priority = MailPriority.High
            };

            using var smtp = new SmtpClient(_emailSetting.PrimaryDomain, _emailSetting.PrimaryPort)
            {
                Credentials = new NetworkCredential(_emailSetting.UsernameEmail, _emailSetting.UsernamePassword),
                EnableSsl = false
            };
            await smtp.SendMailAsync(mail);
        }
    }
}