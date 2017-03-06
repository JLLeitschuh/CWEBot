using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
namespace CWEBot.CLI
{
    [Verb("extract", HelpText = "Extract vulnerabilities data from a data source into a common JSON format.")]
    class ExtractOptions
    {
        [Option('f', "output-file", Required = true, HelpText = "Output data file name for model dataset. A file with .json or .json.gz extension will be created with this name.")]
        public string OutputFile { get; set; }

        [Option('o', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('a', "append", Required = false, Default = false, HelpText = "Append extracted data to existing output file if it exists.")]
        public bool AppendToOutputFile { get; set; }

        [Option('c', "compress", Required = false, Default = false, HelpText = "Output file will be compressed with GZIP.")]
        public bool CompressOutputFile { get; set; }

        [Option('v', "vulnerabilities", Required = false, HelpText = "Limit the number of vulnerabilities extracted.", Default = 0)]
        public int VulnerabilitiesLimit { get; set; }

        [Option('A', "authentication", Required = true, HelpText = "Authentication string (if necessary) that will be used by the selected extractor.")]
        public string Authentication { get; set; }

        [Value(0, Required = true, HelpText = "The extractor to use.")]
        public string Extractor { get; set; }

        [Value(1, Required = false, HelpText = "Any additional parameters for the selected extractor.")]
        public IEnumerable<string> ExtractParameters { get; set; }
    }

    [Verb("transform", HelpText = "Transform extracted vulnerabilities data into the model training/test dataset.")]
    class TransformOptions
    {
        [Value(0, Required = true, HelpText = "Input data file with extracted vulnerability data.")]
        public string InputFile { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name for model dataset. Files with the .tsv extension will be created with this name.")]
        public string OutputFile { get; set; }

        [Option('o', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('s', "split", Required = false, Default = 8, HelpText = "The test data / training data split ratio.")]
        public int Split { get; set; }

        [Option('d', "with-description", Required = false, Default = false, HelpText = "The test data / training data split ration.")]
        public bool WithDescription { get; set; }

        [Option('v', "vulnerabilities", Required = false, HelpText = "Limit the number of vulnerabilities extracted.", Default = 0)]
        public int VulnerabilitiesLimit { get; set; }
    }
}
