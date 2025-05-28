using DmarcTlsReportParser.Models;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace DmarcTlsReportParser
{
    public class DmarcParser
    {
        private readonly string _outputDir;

        public DmarcParser(string outputDir)
        {
            _outputDir = outputDir;
            Directory.CreateDirectory(_outputDir);
        }

        public void Parse(string filePath, List<string> parsedFiles)
        {
            var doc = XDocument.Load(filePath);
            var report = new DmarcReport
            {
                ReportId = Path.GetFileNameWithoutExtension(filePath),
                OrgName = doc.Descendants("org_name").FirstOrDefault()?.Value ?? "Unknown",
                Records = new List<DmarcRecord>()
            };

            foreach (var record in doc.Descendants("record"))
            {
                var sourceIp = record.Element("row")?.Element("source_ip")?.Value ?? "";
                var count = int.Parse(record.Element("row")?.Element("count")?.Value ?? "0");
                var policyEval = record.Element("row")?.Element("policy_evaluated");

                var spf = policyEval?.Element("spf")?.Value;
                var dkim = policyEval?.Element("dkim")?.Value;
                var disposition = policyEval?.Element("disposition")?.Value;

                var passedSpf = spf == "pass";
                var passedDkim = dkim == "pass";
                var passedDmarc = passedSpf && passedDkim;

                report.Records.Add(new DmarcRecord
                {
                    SourceIp = sourceIp,
                    Count = count,
                    Spf = passedSpf,
                    Dkim = passedDkim,
                    Dmarc = passedDmarc,
                    Disposition = disposition
                });
            }

            var outputPath = Path.Combine(_outputDir, $"{report.OrgName}_ReportId_{report.ReportId}_dmarc.json");
            File.WriteAllText(outputPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            parsedFiles.Add(outputPath);
            Console.WriteLine($"Parsed DMARC report written to {outputPath}");
        }
    }
    
}
