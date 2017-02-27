using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWEBot.Interfaces
{
    public interface IExtractor
    {
        string Extract(string file, bool overwrite, int packages, int vulnerabilities, Dictionary<string, string> options);
    }
}
