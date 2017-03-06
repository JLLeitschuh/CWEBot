using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
namespace CWEBot.Extract.OSSIndex
{
    public class Package
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("pm")]
        public string PackageManager { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vulnerabilitytotal")]
        public int VulnerabilityTotal { get; set; }

        [JsonProperty("vulnerabilitymatches")]
        public int VulnerabilityMatches { get; set; }

        [JsonProperty("vulnerabilities")]
        public Vulnerability[] Vulnerabilities { get; set; }
    }
}
