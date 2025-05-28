using DmarcTlsReportParser.Models;
using System.Text;
using System.Text.Json;


namespace DmarcTlsReportParser
{
    public class TlsRptParser
    {
        private readonly string _outputDir;

        public TlsRptParser(string outputDir)
        {
            _outputDir = outputDir;
            Directory.CreateDirectory(_outputDir);
        }

        public void Parse(string filePath, List<string> parsedFiles)
        {
            var json = File.ReadAllText(filePath);
            var report = JsonSerializer.Deserialize<TlsRptReport>(json, new JsonSerializerOptions {PropertyNameCaseInsensitive = true });

            if (report == null)
            {
                Console.WriteLine($"Failed to parse TLS-RPT report: {filePath}");
                return;
            }

            var summaryOutput = new
            {
                report.OrganizationName,
                report.ContactInfo,
                report.ReportId,
                Start = report.DateRange.Start,
                End = report.DateRange.End,
                Policies = report.Policies.Select(p => new
                {
                    Domain = p.Policy.Domain,
                    Type = p.Policy.Type,
                    MxHosts = p.Policy.MxHost,
                    PolicyString = p.Policy.PolicyString,
                    SuccessfulSessions = p.Summary.SuccessCount,
                    FailedSessions = p.Summary.FailureCount
                })
            };

            var dateRangeStr = $"{report.DateRange.Start:dd_MM_yyyy}_to_{report.DateRange.End:dd_MM_yyyy}";
            var outputPath = Path.Combine(_outputDir, $"{report.OrganizationName}_DateRange_{dateRangeStr}_tlsrpt_summary.json");
            File.WriteAllText(outputPath, JsonSerializer.Serialize(summaryOutput, new JsonSerializerOptions { WriteIndented = true }));
            parsedFiles.Add(outputPath);
            Console.WriteLine($"Parsed TLS-RPT summary written to {outputPath}");
        }
    }
}
