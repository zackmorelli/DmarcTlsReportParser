using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmarcTlsReportParser
{
    public class ReportExtractor
    {
        private readonly string _inputDir;
        private readonly string _outputDir;

        public ReportExtractor(string inputDir, string outputDir)
        {
            _inputDir = inputDir;
            _outputDir = outputDir;
        }

        public List<string> ExtractAll()
        {
            var newlyExtractedFiles = new List<string>();

            foreach (var file in Directory.GetFiles(_inputDir))
            {
                try
                {
                    var extension = Path.GetExtension(file).ToLowerInvariant();

                    if (extension == ".zip")
                    {
                        newlyExtractedFiles.AddRange(ExtractZip(file));
                    }
                    else if (extension == ".gz")
                    {
                        newlyExtractedFiles.Add(ExtractGzip(file));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to extract {file}: {ex.Message}");
                }
            }

            return newlyExtractedFiles;
        }

        private List<string> ExtractZip(string zipFilePath)
        {
            var extractedFiles = new List<string>();
            var baseName = Path.GetFileNameWithoutExtension(zipFilePath);
            var extractPath = Path.Combine(_outputDir, baseName);
            Directory.CreateDirectory(extractPath);

            using var archive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in archive.Entries)
            {
                var fullPath = Path.Combine(extractPath, entry.Name);
                entry.ExtractToFile(fullPath, overwrite: true);
                Console.WriteLine($"Extracted ZIP: {entry.Name}");
                extractedFiles.Add(fullPath);
            }
            return extractedFiles;
        }

        private string ExtractGzip(string gzipFilePath)
        {
            var baseName = Path.GetFileNameWithoutExtension(gzipFilePath); // e.g., report.xml or report.json
            var extractPath = Path.Combine(_outputDir, baseName); // keeps .xml or .json

            using var input = File.OpenRead(gzipFilePath);
            using var output = File.Create(extractPath);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            gzip.CopyTo(output);

            Console.WriteLine($"Extracted GZ: {Path.GetFileName(extractPath)}");
            return extractPath;
        }


    }
}
