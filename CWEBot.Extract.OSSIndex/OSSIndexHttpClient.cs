using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Serilog;

namespace CWEBot.Extract.OSSIndex
{
    public class OSSIndexHttpClient
    {
        #region Constructors
        public OSSIndexHttpClient(string api_version, string user = "", string password = "", string server_public_key = "")
        {
            Contract.Requires(string.IsNullOrEmpty(api_version), "api_version must not be null.");
            //Contract.Requires<ArgumentNullException>(string.IsNullOrEmpty(user), "user must not be null.");
            //Contract.Requires<ArgumentNullException>(string.IsNullOrEmpty(password), "password must not be null.");
            this.ApiVersion = api_version;
            this.BaseUrl = string.Format("/v{0}/vulnerability/", this.ApiVersion);
            this.User = user;
            this.Password = password;
            this.ServerPublicKey = "";
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                this.Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", this.User, this.Password)));
            }
        }
        #endregion

        #region Properties
        public string ApiVersion { get; set; }

        public string BaseUrl { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Credentials { get; set; }

        public string ServerPublicKey { get; set; }
        #endregion

        #region Methods
        public async Task<QueryResponse> GetPackages(string package_manager, long from = 0, long till = -1)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://ossindex.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("user-agent", "CWEBot");
                HttpResponseMessage response = await client.GetAsync(string.Format(this.BaseUrl + "pm/{0}/fromtill/{1}/{2}", package_manager, from, till));
                if (response.IsSuccessStatusCode)
                {
                    string r = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<QueryResponse>(r);
                }
                else
                {
                    throw new OSSIndexHttpException(string.Format(this.BaseUrl + "/pm/{0}/{1}/{2}", package_manager, from, till), response.StatusCode, response.ReasonPhrase, response.RequestMessage);
                }
            }
        }
        #endregion
    }
}
