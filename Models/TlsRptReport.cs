
using System.Text.Json.Serialization;

namespace DmarcTlsReportParser.Models
{
    public class TlsRptReport
    {
        [JsonPropertyName("organization-name")]
        public string OrganizationName { get; set; } = "";

        [JsonPropertyName("contact-info")]
        public string ContactInfo { get; set; } = "";

        [JsonPropertyName("report-id")]
        public string ReportId { get; set; } = "";

        [JsonPropertyName("date-range")]
        public DateRange DateRange { get; set; } = new();

        [JsonPropertyName("policies")]
        public List<PolicyEntry> Policies { get; set; } = new();
    }

    public class DateRange
    {
        [JsonPropertyName("start-datetime")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end-datetime")]
        public DateTime End { get; set; }
    }

    public class PolicyEntry
    {
        [JsonPropertyName("policy")]
        public TlsPolicy Policy { get; set; } = new();

        [JsonPropertyName("summary")]
        public TlsSummary Summary { get; set; } = new();
    }

    public class TlsPolicy
    {
        [JsonPropertyName("policy-type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("policy-string")]
        public List<string> PolicyString { get; set; } = new();

        [JsonPropertyName("policy-domain")]
        public string Domain { get; set; } = "";

        [JsonPropertyName("mx-host")]
        public List<string> MxHost { get; set; } = new();
    }

    public class TlsSummary
    {
        [JsonPropertyName("total-successful-session-count")]
        public int SuccessCount { get; set; }

        [JsonPropertyName("total-failure-session-count")]
        public int FailureCount { get; set; }
    }

    public class TlsRptSummary
    {
        public string OrganizationName { get; set; }
        public string ContactInfo { get; set; }
        public string ReportId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<TlsRptPolicySummary> Policies { get; set; }
    }

    public class TlsRptPolicySummary
    {
        public string Domain { get; set; }
        public string Type { get; set; }
        public List<string> MxHosts { get; set; }
        public List<string> PolicyString { get; set; }
        public int SuccessfulSessions { get; set; }
        public int FailedSessions { get; set; }
    }
}
