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
        [JsonProperty("pm")]
        public string PackageManager  { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

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
    }
}
