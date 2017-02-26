using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CWEBot.Extract.OSSIndex
{
    public class ExtractedRecord
    {
        #region Properties
        [JsonProperty("pm")]
        public string PackageManager  { get; set; }

        [JsonProperty("pid")]
        public long PackageId { get; set; }

        [JsonProperty("name")]
        public string PackageName { get; set; }

        [JsonProperty("vid")]
        public long VulnerabilityId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("references")]
        public string[] References { get; set; }

        [JsonProperty("published")]
        public DateTime Published { get; set; }

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
