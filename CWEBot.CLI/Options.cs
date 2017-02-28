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
        [Option('f', "output-file", Required = true, HelpText = "Output data file name for model dataset. Files with .tsv extensions will be created with this name.")]
        public string OutputFile { get; set; }

        [Option("overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option("append", Required = false, Default = false, HelpText = "Append extracted data to existing output file if it exists.")]
        public bool AppendToOutputFile { get; set; }

        [Option('v', "vulnerabilities", Required = false, HelpText = "Limit the number of vulnerabilities extracted.", Default = 0)]
        public int VulnerabilitiesLimit { get; set; }

        [Value(0, Required = true, HelpText = "The extractor to use together with any required parameters.")]
        public IEnumerable<string> ExtractParameters { get; set; }
    }

    [Verb("transform", HelpText = "Transform extracted vulnerabilities data into the model training/test dataset.")]
    class TransformOptions
    {
        [Option('i', "input-file", Required = true, HelpText = "Input data file with extracted vulnerability data.")]
        public string InputFile { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name for model dataset. Files with .tsv extensions will be created with this name.")]
        public string OutputFile { get; set; }

        [Option("overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output file if it exists.")]
        public bool OverwriteOutputFile { get; set; }
    }
}
