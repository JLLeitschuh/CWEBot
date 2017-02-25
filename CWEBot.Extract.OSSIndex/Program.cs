using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;

namespace CWEBot.Extract.OSSIndex
{
    class Program
    {
        static Dictionary<string, string> AppConfig { get; set; }
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile("CWEBot.Extract.OSSIndex-{Date}.txt")
                .CreateLogger();
            AppConfig = Configuration.ReadConfiguration();
            Log.Information("Read {0} values from configuration: {AppConfig}", AppConfig);
            OSSIndexHttpClient client = new OSSIndexHttpClient("2.0");
            QueryResponse packages = client.GetPackages("npm").Result;
        }
    }
}
