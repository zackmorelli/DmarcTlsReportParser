using System.Xml.Linq;

namespace DmarcTlsReportParser
{
    public class ReportClassifier
    {
        private readonly string _inputDir;
        private readonly string _outputDir;

        public List<string> ParsedFiles { get; private set; } = new();

        public ReportClassifier(string inputDir, string outputDir)
        {
            _inputDir = inputDir;
            _outputDir = outputDir;
        }

        public void ClassifyAndParse()
        {
            var dmarcParser = new DmarcParser(_outputDir);
            var tlsRptParser = new TlsRptParser(_outputDir);
            foreach (var file in Directory.EnumerateFiles(_inputDir, "*.*", SearchOption.AllDirectories))
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
