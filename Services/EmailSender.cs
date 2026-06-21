using MailKit.Net.Smtp;
using MimeKit;

namespace Services
{
    public class EmailSender
    {
        private readonly string smtpServer = "smtp-relay.brevo.com";
        private readonly int smtpPort = 587;
        private readonly string smtpUsername = "9539bc001@smtp-brevo.com";
        private readonly string smtpPassword = "mRKHs1DkPQJtqSEf";

        public async Task SendEmail(string senderName, string senderEmail, string toName, string toEmail,
                              string subject, string content)
        {
            try
            {
                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(senderName, senderEmail));
                msg.To.Add(new MailboxAddress(toName, toEmail));
                msg.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = content };
                msg.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
