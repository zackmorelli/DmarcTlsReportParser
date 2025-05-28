
namespace DmarcTlsReportParser.Models
{
    public class DmarcRecord
    {
        public string SourceIp { get; set; } = "";
        public int Count { get; set; }
        public bool Spf { get; set; }
        public bool Dkim { get; set; }
        public bool Dmarc { get; set; }
        public string Disposition { get; set; } = "";
    }
}
