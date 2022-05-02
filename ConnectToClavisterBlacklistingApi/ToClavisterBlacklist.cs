using ConnectToClavisterBlacklisting.Models;
using SyslogServerProject.Models;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ConnectToClavisterBlacklisting
{
    public class ToClavisterBlacklist
    {
        public ToClavisterBlacklist() { }

        /// <summary>
        /// Creates a client with username, password and base address from appsettings
        /// </summary>
        /// <returns>Connection to Clavisters api</returns>
        private HttpClient CreateClient()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =

                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.Default.GetBytes($"{ConfigurationManager.AppSettings["username"]}:{ConfigurationManager.AppSettings["password"]}")));
            client.BaseAddress = new Uri($"{ConfigurationManager.AppSettings.Get("baseUri")}");
            return client;
        }

        /// <summary>
        /// Post a blacklist to Clavister-api
        /// </summary>
        /// <param name="blacklist">parameters for the blacklist</param>
        public void SendToClavisterBlacklist(Blacklist blacklist)
        {
            using (var client = CreateClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("host", blacklist.host_ip),
                          new KeyValuePair<string, string>("service", blacklist.service),
                         new KeyValuePair<string, string>("ttl", blacklist.ttl.ToString()),
                           new KeyValuePair<string, string>("close_established", ChangeBoolToString(blacklist.close_established)),
                        new KeyValuePair<string, string>("rule_name", blacklist.rule_name),
                            new KeyValuePair<string, string>("description", blacklist.description),

                    });

                var result = client.PostAsync("api/oper/blacklist", formContent).Result;
                if (result.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{blacklist.host_ip} added!");
                }
                else
                {
                    Console.WriteLine($"Something went wrong!");
                    Console.WriteLine(result.Content);
                }
            }
        }

        /// <summary>
        /// change bool to yes or no
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>yes or no</returns>
        private string ChangeBoolToString(bool boolean)
        {
            if (boolean) { return "yes"; }
            else { return "no"; }
        }

        /// <summary>
        /// Deletes a blacklist
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task RemoveBlacklist(string param)
        {
            using (var client = CreateClient())
            {
               await client.DeleteAsync($"api/oper/blacklist?{param}");
            }
            
        }

        /// <summary>
        /// Get all blacklisted now, from Clavister-api
        /// </summary>
        /// <param name="alert_type"></param>
        /// <returns>Response from Clavister</returns>
        public async Task<ClavisterBlacklistResponse> ListBlacklist()
        {
            using (var client = CreateClient())
            {
                var blacklisted = await client.GetFromJsonAsync<ClavisterBlacklistResponse>($"api/oper/blacklist");
                return blacklisted;
            }
        }
    }
}