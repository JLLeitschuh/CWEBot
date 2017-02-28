using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using CWEBot.Interfaces;

namespace CWEBot
{
    public class OSSIndexExtractor : Extractor
    {
        public OSSIndexExtractor(FileInfo outputFile, bool overwrite, bool append, ILogger logger) : base(outputFile, overwrite, append, logger)
        {

        }

        #region Overriden methods
        public override int Extract(int vulnerabilitiesLimit, Dictionary<string, string> options)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
