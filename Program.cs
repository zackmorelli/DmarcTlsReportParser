
using System.Diagnostics;

namespace DmarcTlsReportParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            var config = ConfigLoader.Load();

            foreach (var account in config.ImapRetrievalAccounts)
            {
                Console.WriteLine($"Checking {account.Email} for unread DMARC and TLS-RPT zip files...");
                var downloader = new EmailDownloader(account, config.ReportSavePath);
                await downloader.ProcessMailboxAsync();
            }

            Console.WriteLine("\nAll mailboxes processed and DMARC and TLS-RPT zip files saved to disk.");

            var extractor = new ReportExtractor(config.ReportSavePath, config.ExtractedReportsPath);
            extractor.ExtractAll();
            Console.WriteLine("\nDMARC and TLS-RPT zip files extracted.");

            var classifier = new ReportClassifier(config.ExtractedReportsPath, config.GeneratedSummariesPath);
            Console.WriteLine("\nParsing DMARC and TLS-RPT reports...");
            classifier.ClassifyAndParse();

            Console.WriteLine("\nGenerating email report...");
            var emailBodyBuilder = new ReportEmailBuilder(classifier.ParsedFiles); 
            string emailBody = emailBodyBuilder.BuildHtmlSummary();

            Console.WriteLine("\nSending email...");
            var emailSender = new ReportEmailSender();
            string subject = DateTime.Today.ToString("yyyy-MM-dd") + " XXXXXXXXXXXXXXXX DMARC and TLS-RPT Report Summary";
            await emailSender.SendSummaryEmailAsync(config.ImapSendingAccount, subject, emailBody, "XXXXXXXXXXXXXXXX.com");

            Console.WriteLine("Email sent.");
            Console.WriteLine($"Total Execution Time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }
    }
}