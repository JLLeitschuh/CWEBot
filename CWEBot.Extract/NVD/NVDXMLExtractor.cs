using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Serilog;

using CWEBot.Interfaces;
using CWEBot.Models;

namespace CWEBot
{
    public class NVDXMLExtractor : Extractor
    {
        #region Constructirs
        public NVDXMLExtractor(FileInfo outputFile, bool overwrite, bool append, ILogger logger, Dictionary<string, string> options) : base(outputFile, overwrite, append, logger, options)
        {
            Contract.Requires(options != null && options.ContainsKey("InputFile"));
            L = logger.ForContext<NVDXMLExtractor>();
            InputFile = new FileInfo(options["InputFile"]);
            Contract.Requires(InputFile.Exists);
        }
        #endregion

        #region Overriden methods
        public override int Extract(int vulnerabilitiesLimit, Dictionary<string, string> options)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(nvd));
            using (FileStream fs = InputFile.OpenRead())
            using (GZipStream gzs = new GZipStream(fs, CompressionMode.Decompress))
            using (MemoryStream ms = new MemoryStream())
            {
                L.Information("Read {bytes} bytes from compressed file.", fs.Length);
                gzs.CopyTo(ms);
                L.Information("Read {bytes} bytes from decompressed memory stream.", ms.Length);
                ms.Seek(0, SeekOrigin.Begin);
                NVDFeed = (nvd) serializer.Deserialize(ms);
                
            }
            if (NVDFeed == null || NVDFeed.entry == null)
            {
                L.Error("Could not deserialize XML entries from file {file}.", InputFile.FullName);
                return 0;
            }
            else if (NVDFeed.entry.Count() == 0)
            { 
                L.Information("XML file {file} has 0 entries.", InputFile.FullName, NVDFeed.entry.Count());
                return 0;
            }
            int extractedCount = 0;
            foreach (vulnerabilityType v in NVDFeed.entry)
            {
                if (v.cwe == null) continue;
                try
                {
                    ExtractedRecords.Add(new Record
                    {
                        VulnerabilityId = long.Parse(v.id.Replace("-", string.Empty).Replace("CVE", string.Empty)),
                        Title = FilterNonText(v.summary),
                        Description = string.Empty,
                        CWEId = int.Parse(v.cwe.First().id.Replace("CWE-", string.Empty)),
                        References = v.references?.Select(re => re.reference.href).ToArray(),
                        Published = v.publisheddatetimeSpecified ? v.publisheddatetime : DateTime.MinValue
                    });
                    extractedCount++;
                }
                catch (Exception e)
                {
                    L.Warning(e, "Exception thrown attempting to extract vulnerability record from CVE entry {cve}.", v.id);
                }
                if (vulnerabilitiesLimit > 0 && extractedCount > vulnerabilitiesLimit) break;
            }
            L.Information("Extracted {0} vulnerability records with valid CWEs from NVD CVE XML file {1} with total {entries} entries.", extractedCount, InputFile.FullName, NVDFeed.entry.Count());
            return ExtractedRecords.Count;
        }
        #endregion

        #region Properties
        public FileInfo InputFile { get; protected set; }
        protected nvd NVDFeed { get; set; }
        #endregion
    }
}
