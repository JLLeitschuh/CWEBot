using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Serilog;

namespace CWEBot.Interfaces
{
    public abstract class Extractor : IExtractor
    {
        #region Constructors
        public Extractor(FileInfo outputFile, bool overwrite, bool append, ILogger logger, Dictionary<string, object> options = null)
        {
            Contract.Requires(outputFile != null);
            Contract.Requires(logger != null);
            L = logger.ForContext<Extractor>();
            Options = options;
            JsonOutputFile = outputFile;
            Overwrite = overwrite;
            Append = append;
            if (options.ContainsKey("CompressOutputFile"))
            {
                CompressOutputFile = true;
            }
            if (options.ContainsKey("Authentication"))
            {
                Authentication = (string) options["Authentication"];
            }
            if (!CompressOutputFile)
            {
                if (Append && JsonOutputFile.Exists)
                {
                    using (StreamReader f = new StreamReader(JsonOutputFile.OpenRead()))
                    using (JsonTextReader reader = new JsonTextReader(f))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ExtractedRecords = serializer.Deserialize<List<Record>>(reader);
                        L.Information("{file} exists with {r} records and will be appended to.", JsonOutputFile, ExtractedRecords.Count);
                    }
                }
            }
            else
            {
                if (Append && JsonOutputFile.Exists)
                {
                    using (GZipStream gzs = new GZipStream(JsonOutputFile.OpenRead(), CompressionMode.Decompress))
                    using (StreamReader f = new StreamReader(gzs))
                    using (JsonTextReader reader = new JsonTextReader(f))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ExtractedRecords = serializer.Deserialize<List<Record>>(reader);
                        L.Information("{file} exists with {r} records and will be appended to.", JsonOutputFile, ExtractedRecords.Count);
                    }
                }

            }
            

        }
        #endregion

        #region Abstract methods
        public abstract int Extract(int vulnerabilitiesLimit, Dictionary<string, string> options);
        #endregion

        #region Properties
        public ILogger L { get; set; }
        public Dictionary<string, object> Options { get; protected set; }
        public FileInfo JsonOutputFile { get; protected set; }
        public List<Record> ExtractedRecords { get; protected set; } = new List<Record>();
        public bool Overwrite { get; protected set; } = false;
        public bool Append { get; protected set; } = false;
        public bool CompressOutputFile { get; protected set; } = false;
        public string Authentication { get; protected set; } = string.Empty;
        #endregion

        #region Methods
        public virtual bool Save()
        {
            Contract.Requires(ExtractedRecords != null);
            Contract.Requires(ExtractedRecords.Count > 0);
            Contract.Requires(JsonOutputFile != null && JsonOutputFile.Exists);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            if (!CompressOutputFile)
            {
                using (StreamWriter sw = new StreamWriter(JsonOutputFile.FullName, false, Encoding.UTF8))
                {
                    serializer.Serialize(sw, ExtractedRecords);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(JsonOutputFile.FullName, FileMode.Create))
                using (GZipStream gzs = new GZipStream(fs, CompressionMode.Compress))
                using (StreamWriter sw = new StreamWriter(gzs, Encoding.UTF8))
                {
                    serializer.Serialize(sw, ExtractedRecords);
                }
            }
            L.Information("Wrote {0} records to {file}", ExtractedRecords.Count, JsonOutputFile.FullName);
            return true;
        }
        protected static string FilterNonText(string t)
        {
            string text = Regex.Replace(t, @"[\u000A\u000B\u000C\u000D\u2028\u2029\u0085]+", String.Empty); //filter out line terminators
            text = Regex.Replace(text, @"\s+", " "); //compress multiple white space into 1
            text = Regex.Replace(text, @"http[^\s]+", ""); //filter out urls
            text = Regex.Replace(text, @"\[^\s+\]", ""); //filter out [text]
            return new string(text.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray()); //filter out anything non-alphanumeric
        }
        #endregion
    }
}
