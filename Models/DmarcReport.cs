
namespace DmarcTlsReportParser.Models
{
    public class DmarcReport
    {
        public string ReportId { get; set; } = "";
        public string OrgName { get; set; } = "";
        public List<DmarcRecord> Records { get; set; } = new();
    }
}
