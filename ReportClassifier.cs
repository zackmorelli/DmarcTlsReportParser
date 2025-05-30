using System.Xml.Linq;

namespace DmarcTlsReportParser
{
    public class ReportClassifier
    {
        private readonly string _inputDir;
        private readonly string _outputDir;

        public List<string> ParsedFiles { get; private set; } = new();

        public ReportClassifier(string inputDir, string baseOutputDir)
        {
            _inputDir = inputDir;
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            _outputDir = Path.Combine(baseOutputDir, today);
            Directory.CreateDirectory(_outputDir);
        }

        public void ClassifyAndParse(IEnumerable<string> filesToParse)
        {
            var dmarcParser = new DmarcParser(_outputDir);
            var tlsRptParser = new TlsRptParser(_outputDir);
            foreach (var file in filesToParse)
            {
                try
                {
                    if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        dmarcParser.Parse(file, ParsedFiles);
                    }
                    else if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        tlsRptParser.Parse(file, ParsedFiles);
                    }
                    else
                    {
                        Console.WriteLine($"[SKIP] Unknown file type: {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing {file}: {ex.Message}");
                }
            }
        }
    }
}
