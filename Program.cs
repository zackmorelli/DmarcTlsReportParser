
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
                var downloader = new EmailDownloader(account, config.RawZipsPath);
                await downloader.ProcessMailboxAsync();
            }

            Console.WriteLine("\nAll mailboxes processed and DMARC and TLS-RPT zip files saved to disk.");

            var extractor = new ReportExtractor(config.RawZipsPath, config.ExtractedReportsPath);
            var newlyExtractedFiles = extractor.ExtractAll();
            Console.WriteLine("\nDMARC and TLS-RPT zip files extracted.");

            var classifier = new ReportClassifier(config.ExtractedReportsPath, config.GeneratedSummariesPath);
            Console.WriteLine("\nParsing DMARC and TLS-RPT reports...");
            classifier.ClassifyAndParse(newlyExtractedFiles);

            Console.WriteLine("\nGenerating email report...");
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var todaySummaryDir = Path.Combine(config.GeneratedSummariesPath, today);
            var todaysFiles = Directory.GetFiles(todaySummaryDir, "*.json");
            var emailBodyBuilder = new ReportEmailBuilder(todaysFiles.ToList()); 
            string emailBody = emailBodyBuilder.BuildHtmlSummary();

            Console.WriteLine("\nSending email...");
            var emailSender = new ReportEmailSender();
            string subject = DateTime.Today.ToString("yyyy-MM-dd") + " " + config.ServerName + " DMARC and TLS-RPT Report Summary";
            await emailSender.SendSummaryEmailAsync(config.ImapSendingAccount, subject, emailBody, config.ReceivingEmail, config.ServerName);

            Console.WriteLine("Email sent.");
            Console.WriteLine($"Total Execution Time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }
    }
}