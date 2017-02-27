using System;
using System.Collections.Generic;
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
            INPUT_FILE = 2,
            OUTPUT_FILE_EXISTS = 3,
            ERROR_TRANSFORMING_DATA = 4
        }

        static Dictionary<string, string> AppConfig { get; set; }
        static ILogger L;
        static Options ProgramOptions { get; set; }
        static TransformOptions TransformOptions { get; set; }
        static FileInfo InputFile { get; set; }
        static FileInfo TrainingOutputFile { get; set; }
        static FileInfo TestOutputFile { get; set; }
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(Path.Combine("logs", "CWEBot") + "-{Date}.log")
                .CreateLogger();
            L = Log.ForContext<Program>();
            var result = Parser.Default.ParseArguments<Options, TransformOptions>(args)
            .WithNotParsed((IEnumerable<Error> errors) =>
            {
                Log.CloseAndFlush();
                Environment.Exit((int)ExitResult.INVALID_OPTIONS);

            })
            .WithParsed((Options o) => ProgramOptions = o)
            .WithParsed((TransformOptions o) => TransformOptions = o);
            if (!File.Exists(TransformOptions.InputFile))
            {
                L.Error("The input file {0} does not exist.", TransformOptions.InputFile);
                Exit(ExitResult.INPUT_FILE);
            }
            else
            {
                InputFile = new FileInfo(TransformOptions.InputFile);
                L.Information("Using input file {file}.", TransformOptions.InputFile);
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
                    L.Information("Existing file {0} will be overwritten.", TrainingOutputFile.FullName);
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
                    L.Information("Existing file {0} will be overwritten.", TestOutputFile.FullName);
                }
            }

            Transform<IRecord> transform = new Transform<IRecord>(InputFile, TrainingOutputFile, TestOutputFile);
            if (!transform.CreateModelDataset())
            {
                return ExitWithCode(ExitResult.ERROR_TRANSFORMING_DATA);
            }
            else
            {
                Log.CloseAndFlush();
                return (int)ExitResult.SUCCESS;
            }
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
