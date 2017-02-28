using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using CommandLine;

using CWEBot.Interfaces;
namespace CWEBot.CLI
{
    class Program
    {
        public enum ExitResult
        {
            SUCCESS = 0,
            INVALID_OPTIONS = 1,
            INPUT_FILE_ERROR = 2,
            OUTPUT_FILE_EXISTS = 3,
            ERROR_TRANSFORMING_DATA = 4
        }

        static Dictionary<string, string> AppConfig { get; set; }
        static ILogger L;
        static List<string> Extractors { get; set; } = new List<string> { "nvd", "ossi" };
        static ExtractOptions ExtractOptions { get; set; }
        static Extractor Extractor { get; set; }
        static TransformOptions TransformOptions { get; set; }
        static FileInfo InputFile { get; set; }
        static FileInfo ExtractOutputFile { get; set; }
        static FileInfo TrainingOutputFile { get; set; }
        static FileInfo TestOutputFile { get; set; }
        static FileInfo TargetOutputFile { get; set; }
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(Path.Combine("logs", "CWEBot") + "-{Date}.log")
                .CreateLogger();
            L = Log.ForContext<Program>();
            var result = Parser.Default.ParseArguments<ExtractOptions, TransformOptions>(args)
            .WithNotParsed((IEnumerable<Error> errors) =>
            {
                Exit(ExitResult.INVALID_OPTIONS);

            })
            .WithParsed((ExtractOptions o) =>
            {
                if (o.ExtractParameters.Count() > 0 && Extractors.Contains(o.ExtractParameters.First()))
                {
                    ExtractOptions = o;
                    Extract();
                    Exit(ExitResult.SUCCESS);
                }
                else
                {
                    L.Error("The current extractors are: {extractors}.", Extractors);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                
            })
            .WithParsed((TransformOptions o) =>
            {
                TransformOptions = o;
                if (!File.Exists(TransformOptions.InputFile))
                {
                    L.Error("The input file {0} for transform does not exist.", TransformOptions.InputFile);
                    Exit(ExitResult.INPUT_FILE_ERROR);
                }
                else
                {
                    InputFile = new FileInfo(TransformOptions.InputFile);
                    L.Information("Using input file {file} or transform.", TransformOptions.InputFile);
                }

                TrainingOutputFile = new FileInfo(TransformOptions.OutputFile + ".training.tsv");
                if (TrainingOutputFile.Exists)
                {
                    if (!TransformOptions.OverwriteOutputFile)
                    {
                        L.Error("The training output file {0} exists. Use the --overwrite flag to overwrite an existing file.", TrainingOutputFile.FullName);
                        Exit(ExitResult.OUTPUT_FILE_EXISTS);
                    }
                    else
                    {
                        L.Information("Existing training output file {0} will be overwritten.", TrainingOutputFile.FullName);
                    }
                }

                TestOutputFile = new FileInfo(TransformOptions.OutputFile + ".test.tsv");
                if (TrainingOutputFile.Exists)
                {
                    if (!TransformOptions.OverwriteOutputFile)
                    {
                        L.Error("The test output file {0} exists. Use the --overwrite flag to overwrite an existing file.", TestOutputFile.FullName);
                        Exit(ExitResult.OUTPUT_FILE_EXISTS);
                    }
                    else
                    {
                        L.Information("Existing test output file {0} will be overwritten.", TestOutputFile.FullName);
                    }
                }

                TargetOutputFile = new FileInfo(TransformOptions.OutputFile + ".target.tsv");
                if (TargetOutputFile.Exists)
                {
                    if (!TransformOptions.OverwriteOutputFile)
                    {
                        L.Error("The target output file {0} exists. Use the --overwrite flag to overwrite an existing file.", TargetOutputFile.FullName);
                        Exit(ExitResult.OUTPUT_FILE_EXISTS);
                    }
                    else
                    {
                        L.Information("Existing target dataset file {0} will be overwritten.", TargetOutputFile.FullName);
                    }
                }

                TransformStage transform = new TransformStage(InputFile, TrainingOutputFile, TestOutputFile, TargetOutputFile);
                if (!transform.CreateModelDataset())
                {
                    Exit(ExitResult.ERROR_TRANSFORMING_DATA);
                }
                else
                {
                    Exit(ExitResult.SUCCESS);
                }
            });
            return 0;
          
        }

        static bool Extract()
        {
            ExtractOutputFile = new FileInfo(ExtractOptions.OutputFile);
            if (ExtractOutputFile.Exists)
            {
                if (!(ExtractOptions.OverwriteOutputFile || ExtractOptions.AppendToOutputFile))
                {
                    L.Error("The json output file {0} exists. Use the --overwrite flag to overwrite an existing file or --append to append extracted records to the existing file.", ExtractOutputFile.FullName);
                    Exit(ExitResult.OUTPUT_FILE_EXISTS);
                }
                else if (ExtractOptions.OverwriteOutputFile)
                {
                    L.Information("Existing file {0} will be overwritten.", ExtractOutputFile.FullName);
                }
                else
                {
                    L.Information("Existing file {0} will be appended to.", ExtractOutputFile.FullName);
                }
            }
            else
            {
                L.Information("Using JSON output file {0}.", ExtractOutputFile.FullName);
            }
            ExtractStage e = new ExtractStage(ExtractOptions.ExtractParameters.First(), ExtractOutputFile, ExtractOptions.OverwriteOutputFile, ExtractOptions.AppendToOutputFile,
                L, ExtractOptions.ExtractParameters.ToList());
            e.Run(ExtractOptions.VulnerabilitiesLimit, null);
            return true;

        }
        static void Exit(ExitResult result)
        {
            Log.CloseAndFlush();
            Environment.Exit((int)result);
        }

        static int ExitWithCode(ExitResult result)
        {
            Log.CloseAndFlush();
            return (int) result;
        }
    }
}
