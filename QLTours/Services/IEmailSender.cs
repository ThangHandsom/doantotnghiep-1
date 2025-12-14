using System.Net.Mail;
using System.Net;

namespace QLTours.Services
{
    // Định nghĩa interface IEmailSender
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string smtpUsername;
        private readonly string smtpPassword;

        public SmtpEmailSender(string host, int port, string username, string password)
        {
            smtpHost = host;
            smtpPort = port;
            smtpUsername = username;
            smtpPassword = password;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true; 
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }

}