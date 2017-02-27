using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CWEBot.Interfaces
{
    public interface IRecord
    {
        [JsonProperty("vid")]
        long VulnerabilityId { get; set; }

        [JsonProperty("title")]
        string Title { get; set; }

        [JsonProperty("description")]
        string Description { get; set; }

        [JsonProperty("references")]
        string[] References { get; set; }

        [JsonProperty("Published")]
        DateTime Published { get; set; }

        int? CWEId { get; set; }
    }
}
