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
        static FileInfo JsonOutputFile { get; set; }
        static FileInfo TrainOutputFile { get; set; }
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
            JsonOutputFile = new FileInfo(ProgramOptions.OutputFile + ".json");
            if (JsonOutputFile.Exists)
            {
                if (!ProgramOptions.OverwriteOutputFile)
                {
                    L.Error("The json output file {0} exists. Use the --overwrite flag to overwrite an existing file.", JsonOutputFile.FullName);
                    Exit(ExitResult.OUTPUT_FILE_EXISTS);
                }
                else
                {
                    L.Information("Existing file {0} will be overwritten.", JsonOutputFile.FullName);
                }
            }
            else
            {
                L.Information("Using JSON output file {0}.", JsonOutputFile.FullName);
            }
            TrainOutputFile = new FileInfo(ProgramOptions.OutputFile + ".training.tsv");
            if (TrainOutputFile.Exists)
            {
                if (!ProgramOptions.OverwriteOutputFile)
                {
                    L.Error("The training output file {0} exists. Use the --overwrite flag to overwrite an existing file.", TrainOutputFile.FullName);
                    Exit(ExitResult.OUTPUT_FILE_EXISTS);
                }
                else
                {
                    L.Information("Existing file {0} will be overwritten.", TrainOutputFile.FullName);
                }
            }
            else
            {
                L.Information("Using training output file {0}.", TrainOutputFile.FullName);
            }
            OSSIndexHttpClient client = new OSSIndexHttpClient("2.0");
            List<ExtractedRecord> records = new List<ExtractedRecord>();
            VulnerablityComparator vc = new VulnerablityComparator();
            ExtractedRecordComparator erc = new ExtractedRecordComparator();
            foreach (string pm in ProgramOptions.PackageManager)
            {
                bool hasNext = false;
                long from = 0, till = -1;
                do
                {
                    QueryResponse response = client.GetPackages(pm, from, till).Result;
                    L.Information("Got {ps} package entries with {vuln} distinct vulnerability entries for package manager {pm}.", 
                        response.packages.Where(p => p.PackageManager == pm).Select(p => p.Id).Distinct().Count(),
                        response.packages.Where(p => p.PackageManager == pm).SelectMany(p => p.Vulnerabilities).Distinct(vc).Count(), pm);
                    hasNext = !string.IsNullOrEmpty(response.NextUrl);
                    var duplicates = response.packages.SelectMany(p => p.Vulnerabilities).GroupBy(v => v.Id)
                        .Where(g => g.Count() > 1)
                        .Select(y => y.Key)
                        .ToList();
                    L.Information("Got {0} duplicate vulnerabilities.", duplicates.Count);
                    foreach (Package package in response.packages)
                    {
                        records.AddRange(
                            package.Vulnerabilities.Select(v => new 
                            ExtractedRecord
                            {
                                PackageManager = pm,
                                PackageId = package.Id,
                                PackageName = package.Name,
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
                    if (ProgramOptions.PackagesLimit > 0 && records.Distinct(erc).Where(r => r.PackageManager == pm).Select(r => r.PackageId).Distinct().Count() > ProgramOptions.PackagesLimit)
                    {
                        hasNext = false;
                    }
                    if (ProgramOptions.VulnerabilitiesLimit > 0 && records.Distinct(erc).Where(r => r.PackageManager == pm).Count() > ProgramOptions.VulnerabilitiesLimit)
                    {
                        hasNext = false;
                    }
                }
                while (hasNext); 
            }
            List<ExtractedRecord> extracted = records.Distinct(erc).ToList();
            L.Information("Extracted {packages} packages with {vulnd} distinct vulnerabilities.", extracted.Select(r => r.PackageId).Distinct().Count(),
                      extracted.Count());
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(JsonOutputFile.FullName, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, extracted);
            }
            L.Information("Wrote {extracted} records to {file}.", extracted.Count, JsonOutputFile.FullName);

            
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
