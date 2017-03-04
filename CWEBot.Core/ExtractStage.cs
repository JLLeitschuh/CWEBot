using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Newtonsoft.Json;

using CWEBot.Interfaces;

namespace CWEBot
{
    public class ExtractStage
    {
        public ExtractStage(string extractor, FileInfo outputFile, bool overwrite, bool append, ILogger logger, List<string> parameters)
        {
            L = logger.ForContext<ExtractStage>();
            if (extractor == "nvd")
            {
                if (parameters.Count < 1)
                {
                    L.Error("At least 1 parameter is needed for the NVD extractor: the input file.");
                    throw new ArgumentException("At least 1 parameter is needed for the NVD extractor: the input file.", "parameters");
                }
                else if (!File.Exists(parameters[0]))
                {
                    L.Error("The input file parameter {file} for the NVD extractor does not exist.", parameters[0]);
                    throw new ArgumentException("The input file parameter for the NVD extractor does not exist.", "parameters");
                }
                Dictionary<string, object> options = new Dictionary<string, object> { { "InputFile", parameters[0] } };
                if (parameters.Contains("CompressOutputFile"))
                {
                    options.Add("CompressOutputFile", true);
                }
                Extractor = new NVDXMLExtractor(outputFile, overwrite, append, L, options);
            }
            else if (extractor == "ossi")
            {
                if (parameters.Count < 1)
                {
                    L.Error("At least 1 parameter is needed for the OSSI extractor: the package manager.");
                    throw new ArgumentException("At least 1 parameter is needed for the OSSI extractor: the package manager.", "parameters");
                }
            
                Dictionary<string, object> options = new Dictionary<string, object> { { "PackageManager", parameters[0] } };
                int packagesLimit;
                if ((options.Count > 1) && !int.TryParse(parameters[1], out packagesLimit))
                {
                    throw new ArgumentException("The 2nd parameter is the packages limit and must be an integer.", "parameters");
                } 
                else if ((options.Count > 1) && int.TryParse(parameters[1], out packagesLimit))
                {
                    options.Add("PackagesLimit", packagesLimit);

                }
                if (parameters.Contains("CompressOutputFile"))
                {
                    options.Add("CompressOutputFile", true);
                }
                Extractor = new OSSIndexExtractor(outputFile, overwrite, append, L, options);
            }
        }

        #region Properties
        ILogger L;
        public Extractor Extractor { get; set; }
        #endregion

        #region Methods
        public bool Run(int vulnerabilitiesLimit, Dictionary<string, string> options)
        { 
            Contract.Requires(Extractor != null);
            if (Extractor.Extract(vulnerabilitiesLimit, options) > 0)
            {
                Extractor.Save();
            }
            else
            {
                L.Information("o records extracted.");
            }
            return true;
        }
        #endregion
    }


}
