﻿using System;
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
                Extractor = new NVDXMLExtractor(outputFile, overwrite, append, L, new Dictionary<string, string> { { "InputFile", parameters[1] } });
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