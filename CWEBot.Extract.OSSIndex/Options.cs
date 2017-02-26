using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
namespace CWEBot.Extract.OSSIndex
{
    class Options
    {
        [Option('f', "file", Required = true, HelpText = "Output data file name. Files with .json and .txt extensions will be created with this extension.")]
        public string OutputFile { get; set; }

        [Option("overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('m', "package-manager", Required = true, HelpText = "The name(s) of the package manager to extract vulnerabilities for.", Separator = ',',
            Default = new string[] {"all"}, Min = 1)]
        public IEnumerable<string> PackageManager { get; set; }

        [Option('v', "vulnerabilities", Required = false, HelpText = "Limit the number of vulnerabilities extracted.", Default = 0)]
        public int VulnerabilitiesLimit { get; set; }

        [Option('p', "packages", Required = false, HelpText = "Limit the number of packages extracted.", Default = 0)]
        public int PackagesLimit { get; set; }
    }

}
