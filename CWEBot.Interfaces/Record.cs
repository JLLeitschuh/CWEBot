using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CWEBot.Interfaces
{
    public class Record : IRecord
    {
        public long VulnerabilityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string[] References { get; set; }

        public DateTime Published { get; set; }

        public int? CWEId { get; set; }
    }
}
