using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWEBot.Interfaces
{
    public interface IExtractor
    {
        int Extract(int vulnerabilitiesLimit, Dictionary<string, string> options);
        bool Save();
    }
}
