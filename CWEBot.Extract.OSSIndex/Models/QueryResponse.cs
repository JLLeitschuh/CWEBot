using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CWEBot.Extract.OSSIndex
{
    public class QueryResponse
    {
        [JsonProperty("packages")]
        public Package[] packages { get; set; }

        [JsonProperty("requestedFrom")]
        public long? RequestedFromTS { get; set; }

        [JsonIgnore]
        public DateTime? RequestedFrom
        {
            get
            {
                if (RequestedFromTS.HasValue)
                {
                    try
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(RequestedFromTS.Value).UtcDateTime;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return DateTime.MinValue;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else return null;
            }
        }

        [JsonProperty("requestedTill")]
        public long? RequestedTillTS { get; set; }

        [JsonIgnore]
        public DateTime? RequestedTill
        {
            get
            {
                if (RequestedTillTS.HasValue)
                {
                    try
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(RequestedTillTS.Value).UtcDateTime;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return DateTime.MaxValue;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else return null;
            }
        }

        [JsonProperty("actualFrom")]
        public long? ActualFromTS { get; set; }


        [JsonIgnore]
        public DateTime? ActualFrom
        {
            get
            {

                if (ActualFromTS.HasValue)
                {
                    try
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(ActualFromTS.Value).UtcDateTime;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return DateTime.MinValue;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else return null;
            }
        }

        [JsonProperty("next")]
        public string NextUrl { get; set; }
    }

    

    

}
