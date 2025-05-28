using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using DmarcTlsReportParser.Models;

namespace DmarcTlsReportParser
{
    public class ReportEmailBuilder
    {
        private readonly List<string> _reportFiles;

        public ReportEmailBuilder(List<string> reportFiles)
        {
            _reportFiles = reportFiles;
        }

        public string BuildHtmlSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body>");

            var dmarcSummaryFiles = _reportFiles
                .Where(f => f.EndsWith("dmarc.json", StringComparison.OrdinalIgnoreCase));

            var tlsRptSummaryFiles = _reportFiles
                .Where(f => f.EndsWith("tlsrpt_summary.json", StringComparison.OrdinalIgnoreCase));

            AppendDmarcReports(sb, dmarcSummaryFiles);
            AppendTlsRptReports(sb, tlsRptSummaryFiles);

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private void AppendDmarcReports(StringBuilder sb, IEnumerable<string> dmarcFiles)
        {
            if (!dmarcFiles.Any()) return;

            sb.AppendLine("<h2>DMARC Reports</h2>");

            foreach (var file in dmarcFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var report = JsonSerializer.Deserialize<DmarcReport>(content);
                    if (report == null) continue;

                    sb.AppendLine($"<h3>{WebUtility.HtmlEncode(report.OrgName)} - {WebUtility.HtmlEncode(report.ReportId)}</h3>");
                    sb.AppendLine("<table border='1' cellpadding='4' cellspacing='0'>");
                    sb.AppendLine("<tr><th>IP</th><th>Count</th><th>SPF</th><th>DKIM</th><th>DMARC</th><th>Disposition</th></tr>");

                    foreach (var record in report.Records)
                    {
                        var whoisInfo = GetWhoisInfo(record.SourceIp);
                        sb.AppendLine("<tr>" +
                            $"<td>{record.SourceIp}<br/><small>{WebUtility.HtmlEncode(whoisInfo)}</small></td>" +
                            $"<td>{record.Count}</td>" +
                            $"<td>{(record.Spf ? "✅" : "❌")}</td>" +
                            $"<td>{(record.Dkim ? "✅" : "❌")}</td>" +
                            $"<td>{(record.Dmarc ? "✅" : "❌")}</td>" +
                            $"<td>{record.Disposition}</td>" +
                            "</tr>");
                    }

                    sb.AppendLine("</table><br/>");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process DMARC file '{file}': {ex.Message}");
                }
            }
        }

        private void AppendTlsRptReports(StringBuilder sb, IEnumerable<string> tlsRptFiles)
        {
            if (!tlsRptFiles.Any()) return;

            sb.AppendLine("<h2>TLS-RPT Reports</h2>");
            foreach (var file in tlsRptFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var report = JsonSerializer.Deserialize<TlsRptSummary>(content);
                    if (report == null) continue;

                    sb.AppendLine($"<h3>{WebUtility.HtmlEncode(report.OrganizationName)} - {WebUtility.HtmlEncode(report.ReportId)}</h3>");
                    sb.AppendLine($"<p>Date Range: {report.Start:yyyy-MM-dd} to {report.End:yyyy-MM-dd}</p>");
                    sb.AppendLine("<table border='1' cellpadding='4' cellspacing='0'>");
                    sb.AppendLine("<tr><th>MX Host(s)</th><th>Domain</th><th>Type</th><th>Success</th><th>Failure</th><th>Mode</th></tr>");

                    foreach (var policy in report.Policies)
                    {
                        string mx = string.Join("<br>", policy.MxHosts);
                        string mode = ParsePolicyMode(policy.PolicyString);
                        string rowStyle = policy.FailedSessions > 0 ? " style='background-color:#ffcccc;'" : "";
                        sb.AppendLine("<tr" + rowStyle + ">" +
                            $"<td>{mx}</td>" +
                            $"<td>{policy.Domain}</td>" +
                            $"<td>{policy.Type}</td>" +
                            $"<td>{policy.SuccessfulSessions}</td>" +
                            $"<td>{policy.FailedSessions}</td>" +
                            $"<td>{mode}</td>" +
                            "</tr>");
                    }

                    sb.AppendLine("</table><br/>");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process TLS-RPT file '{file}': {ex.Message}");
                }
            }
        }

        private string ParsePolicyMode(List<string> lines)
        {
            return lines.FirstOrDefault(l => l.StartsWith("mode:"))?.Split(':')[1].Trim() ?? "unknown";
        }


        private static string GetWhoisInfo(string ipAddress)
        {
            try
            {
                // Step 1: Query IANA to get the appropriate registry
                using var client = new TcpClient("whois.iana.org", 43);
                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                writer.WriteLine(ipAddress);

                using var reader = new StreamReader(stream);
                string response = reader.ReadToEnd();

                var match = Regex.Match(response, @"refer:\s*(\S+)", RegexOptions.IgnoreCase);
                var whoisServer = match.Success ? match.Groups[1].Value : "whois.arin.net";

                // Step 2: Query the actual WHOIS server
                using var client2 = new TcpClient(whoisServer, 43);
                using var stream2 = client2.GetStream();
                using var writer2 = new StreamWriter(stream2) { AutoFlush = true };
                writer2.WriteLine(ipAddress);

                using var reader2 = new StreamReader(stream2);
                var fullWhois = reader2.ReadToEnd();

                // Step 3: Extract useful fields (OrgName or descr + country)
                var orgMatch = Regex.Match(fullWhois, @"(?i)(OrgName|descr):\s*(.+)");
                var countryMatch = Regex.Match(fullWhois, @"(?i)Country:\s*(.+)");

                var org = orgMatch.Success ? orgMatch.Groups[2].Value.Trim() : "Unknown";
                var country = countryMatch.Success ? countryMatch.Groups[1].Value.Trim() : null;

                return country != null ? $"{org} ({country})" : org;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WHOIS failed for {ipAddress}: {ex.Message}");
                return "Unknown";
            }
        }

    }
}
