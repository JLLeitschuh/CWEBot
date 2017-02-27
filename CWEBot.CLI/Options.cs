using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
namespace CWEBot.CLI
{
    class Options
    {
        
    }

    [Verb("transform", HelpText = "Transform extracted vulnerabilities data into the model training/test dataset.")]
    class TransformOptions
    {
        [Option('i', "input-file", Required = true, HelpText = "Input data file with extracted vulnerability data.")]
        public string InputFile { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name for model dataset. Files with .tsv extensions will be created with this namr.")]
        public string OutputFile { get; set; }

        [Option("overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output file if it exists.")]
        public bool OverwriteOutputFile { get; set; }
    }
}
