using DmarcTlsReportParser.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DmarcTlsReportParser
{
    public class EmailDownloader
    {
        private readonly RetrievalAccount _account;
        private readonly string _savePath;

        public EmailDownloader(RetrievalAccount account, string savePath)
        {
            _account = account;
            _savePath = savePath;
        }

        public async Task ProcessMailboxAsync()
        {
            using var client = new ImapClient();
            await client.ConnectAsync(_account.Server, 993, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_account.Email, _account.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadWrite);

            //Note: SearchQuery.NotSeen means unread emails on the email server.
            foreach (var uid in inbox.Search(MailKit.Search.SearchQuery.NotSeen))
            {
                var message = await inbox.GetMessageAsync(uid);

                if (!message.Subject.Contains("Report Domain", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
                foreach (var attachment in message.Attachments)
                {
                    if (attachment is MimePart part && (part.FileName.EndsWith(".zip") || part.FileName.EndsWith(".gz")))
                    {
                        var path = Path.Combine(_savePath, part.FileName); 
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                        using var stream = File.Create(path);
                        await part.Content.DecodeToAsync(stream);
                        Console.WriteLine($"Saved: {part.FileName}");
                    }
                }
            }

            await client.DisconnectAsync(true);
        }
    }
}