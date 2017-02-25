using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

using Serilog;
using Newtonsoft.Json;

namespace CWEBot.Extract.OSSIndex
{
    class Program
    {
        public enum ExitResult
        {
            SUCCESS = 0,
            INVALID_OPTIONS = 1,
            OUTPUT_FILE_EXISTS = 2
        }
        
        static Dictionary<string, string> AppConfig { get; set; }
        static ILogger L;
        static Options ProgramOptions { get; set; }
        static FileInfo OutputFile { get; set; }
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(Path.Combine("logs", "CWEBot.Extract.OSSIndex") + "-{Date}.log")
                .CreateLogger();
            L = Log.ForContext<Program>();            
            ParserResult<Options> options = Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed((IEnumerable<Error> errors) =>
                {
                    Log.CloseAndFlush();
                    Environment.Exit((int)ExitResult.INVALID_OPTIONS);

                })
                .WithParsed((Options o) => ProgramOptions = o);
            AppConfig = Configuration.ReadConfiguration();
            L.Information("Read {0} values from configuration: {AppConfig}", AppConfig.Count, AppConfig);
            OutputFile = new FileInfo(ProgramOptions.OutputFile);
            if (OutputFile.Exists)
            {
                if (!ProgramOptions.OverwriteOutputFile)
                {
                    L.Error("The output file {0} exists. Use the --overwrite flag to overwrite an existing file.", OutputFile.FullName);
                    Exit(ExitResult.OUTPUT_FILE_EXISTS);
                }
                else
                {
                    L.Information("Existing file {0} will be overwritten.", OutputFile.FullName);
                }
            }
            else
            {
                L.Information("Using output file {0}.", OutputFile.FullName);
            }
            OSSIndexHttpClient client = new OSSIndexHttpClient("2.0");
            List<ExtractedRecord> records = new List<ExtractedRecord>();
            foreach (string pm in ProgramOptions.PackageManager)
            {
                bool hasNext = false;
                long from = 0, till = -1;
                do
                {
                    QueryResponse response = client.GetPackages(pm, from, till).Result;
                    L.Information("Got {ps} package entries with {vuln} vulnerability entries for package manager {pm}.", 
                        response.packages.Where(p => p.PackageManager == pm).Select(p => p.Id).Distinct().Count(),
                        response.packages.Where(p => p.PackageManager == pm).SelectMany(p => p.Vulnerabilities).Distinct().Count(), pm);
                    hasNext = !string.IsNullOrEmpty(response.NextUrl);
                    foreach (Package package in response.packages)
                    {
                        records.AddRange(
                            package.Vulnerabilities.Select(v => new 
                            ExtractedRecord
                            {
                                PackageManager = pm,
                                PackageId = package.Id,
                                VulnerabilityId = v.Id,
                                Title = v.Title,
                                Description = v.Description,
                                References = v.References,
                                Updated = v.Updated.HasValue ? v.Updated.Value : DateTime.MinValue,
                                Published = v.Published.HasValue ? v.Published.Value : DateTime.MinValue
                            })
                        );
                    }
                    hasNext = !string.IsNullOrEmpty(response.NextUrl);
                    if (hasNext)
                    { 
                        Uri n = new Uri(response.NextUrl);
                        till = Int64.Parse(n.Segments[7]);
                    }
                    if (ProgramOptions.PackagesLimit > 0 && records.Where(r => r.PackageManager == pm).Select(r => r.PackageId).Distinct().Count() > ProgramOptions.PackagesLimit)
                    {
                        hasNext = false;
                    }
                    if (ProgramOptions.VulnerabilitiesLimit > 0 && records.Where(r => r.PackageManager == pm).Select(r => r.VulnerabilityId).Distinct().Count() > ProgramOptions.VulnerabilitiesLimit)
                    {
                        hasNext = false;
                    }
                }
                while (hasNext); 
            }
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(OutputFile.FullName, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, records);
                
            }
            L.Information("Extracted {packages} packages with {vulnt} total and {vulnd} distinct vulnerabilities to output file {f}", records.Select(r => r.PackageId).Distinct().Count(),
                       records.Select(r => r.VulnerabilityId).Count(), records.Select(r => r.VulnerabilityId).Distinct().Count(), OutputFile.FullName);
            var duplicates = records.GroupBy(x => new { pid = x.PackageId, vid = x.VulnerabilityId })
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
            L.Information("Found {0} duplicates: {dup}.", duplicates.Count, duplicates);
            return (int)ExitResult.SUCCESS;
        }

        static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            L.Fatal(e.ExceptionObject as Exception, "Extract will now terminate.");
            Log.CloseAndFlush();
        }

        static void Exit(ExitResult result)
        {
            Log.CloseAndFlush();
            Environment.Exit((int)result);
        }
    }
}
