using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using DmarcTlsReportParser.Models;

namespace DmarcTlsReportParser
{
    public class ReportEmailSender
    {

        public async Task SendSummaryEmailAsync(EmailAccount sendingAccount, string subject, string htmlBody, string recipient, string emailServer)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(sendingAccount.Username, sendingAccount.Username));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailServer, 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
            await smtp.AuthenticateAsync(sendingAccount.Username, sendingAccount.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            Console.WriteLine("Summary email sent.");
        }
    }
}
