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
using CWEBot.Extract.OSSIndex;

namespace CWEBot
{
    public class OSSIndexExtractor : Extractor
    {
        #region Constructors
        public OSSIndexExtractor(FileInfo outputFile, bool overwrite, bool append, ILogger logger, Dictionary<string, object> options) : base(outputFile, overwrite, append, logger, options)
        {
            Contract.Requires(options != null && options.ContainsKey("PackageManager"));
            L = logger.ForContext<OSSIndexExtractor>();
            PackageManager = (string) options["PackageManager"];
            if (options.ContainsKey("PackagesLimit"))
            {
                PackagesLimit = (int)options["PackagesLimit"];
            }
            if (!string.IsNullOrEmpty(Authentication))
            {
                string[] a = Authentication.Split(":".ToCharArray());
                if (a.Length != 2)
                {
                    throw new ArgumentException("The authentication string is not in the correct format.");
                }
                else
                {
                    User = a[0];
                    Token = a[1];
                }
            }
        }
        #endregion

        #region Properties
        public string PackageManager { get; protected set; }
        public int PackagesLimit { get; protected set; }
        public string User { get; protected set; } = string.Empty;
        public string Token { get; protected set; } = string.Empty;
        #endregion

        #region Overriden Methods
        public override int Extract(int vulnerabilitiesLimit, Dictionary<string, string> options)
        {
            OSSIndexHttpClient client = null;
            if (!string.IsNullOrEmpty(Authentication))
            {
                
                client = new OSSIndexHttpClient("2.0", User, Token);
            }
            else
            {
                client = new OSSIndexHttpClient("2.0");
            }
            List<ExtractedRecord> records = new List<ExtractedRecord>();
            VulnerablityComparator vc = new VulnerablityComparator();
            ExtractedRecordComparator erc = new ExtractedRecordComparator();
            bool hasNext = false;
            long from = 0, till = -1;
            do
            {
                QueryResponse response = client.GetPackages(PackageManager, from, till).Result;
                L.Information("Got {ps} package entries with {vuln} distinct vulnerability entries for package manager {pm}.",
                    response.packages.Select(p => p.Id).Distinct().Count(),
                    response.packages.SelectMany(p => p.Vulnerabilities).Distinct(vc).Count(), PackageManager);
                hasNext = !string.IsNullOrEmpty(response.NextUrl);
                var duplicates = response.packages.SelectMany(p => p.Vulnerabilities).GroupBy(v => v.Id)
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key)
                    .ToList();
                L.Information("Got {0} duplicate vulnerabilities.", duplicates.Count);
                foreach (Package package in response.packages)
                {
                    records.AddRange(
                        package.Vulnerabilities.Select(v => new
                        ExtractedRecord
                        {
                            PackageManager = this.PackageManager,
                            PackageId = package.Id,
                            PackageName = package.Name,
                            VulnerabilityId = v.Id,
                            Title = FilterNonText(v.Title),
                            Description = FilterNonText(v.Description), //strip out any URLs
                            References = v.References,
                            Updated = v.Updated.HasValue ? v.Updated.Value : DateTime.MinValue,
                            Published = v.Published.HasValue ? v.Published.Value : DateTime.MinValue
                        })
                    );
                }
                hasNext = !string.IsNullOrEmpty(response.NextUrl);
                if (hasNext)
                {
                    Uri n = new Uri(response.NextUrl);
                    till = Int64.Parse(n.Segments[7]);
                }
                if (PackagesLimit > 0 && records.Distinct(erc).Select(r => r.PackageId).Distinct().Count() > PackagesLimit)
                {
                    hasNext = false;
                }
                if (vulnerabilitiesLimit > 0 && records.Distinct(erc).Count() > vulnerabilitiesLimit)
                {
                    hasNext = false;
                }
            }
            while (hasNext);
            
            ExtractedRecords = records.Distinct(erc).Select(er => (Record) er).ToList();
            L.Information("Extracted {packages} packages with {vulnd} distinct vulnerabilities.", records.Distinct(erc).Select(r => r.PackageId).Distinct().Count(),
                      ExtractedRecords.Count());
            return ExtractedRecords.Count;
        }
        #endregion
    }
}
