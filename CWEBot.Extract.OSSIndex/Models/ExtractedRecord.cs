using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using CWEBot.Interfaces;
namespace CWEBot.Extract.OSSIndex
{
    public class ExtractedRecord : IRecord
    {
        #region Properties
        [JsonProperty("pm")]
        public string PackageManager  { get; set; }

        [JsonProperty("pid")]
        public long PackageId { get; set; }

        [JsonProperty("name")]
        public string PackageName { get; set; }
        
        public long VulnerabilityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string[] References { get; set; }

        public DateTime Published { get; set; }

        [JsonIgnore]
        public int? CWEId { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
        #endregion
    }

    public class ExtractedRecordComparator : IEqualityComparer<ExtractedRecord>
    {
        public bool Equals(ExtractedRecord r1, ExtractedRecord r2)
        {
            return r1.VulnerabilityId == r2.VulnerabilityId;

        }

        public int GetHashCode(ExtractedRecord r)
        {
            return r.VulnerabilityId.GetHashCode();
        }
    }
}
