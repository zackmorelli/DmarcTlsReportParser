namespace DmarcTlsReportParser.Models
{
    public class AppConfig
    {
        public string ReportSavePath { get; set; }
        public string ExtractedReportsPath { get; set; }
        public string GeneratedSummariesPath { get; set; }
        public List<RetrievalAccount> ImapRetrievalAccounts { get; set; } = new();
        public EmailAccount ImapSendingAccount { get; set; } = new();
    }

    public class RetrievalAccount
    {
        public string Server { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class EmailAccount
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Server { get; set; } = "";
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public bool IsSender { get; set; }
    }
}
