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

        public void SendToClavisterBlacklist(string ip)
        {
            using (var client = CreateClient())
            {             
                try
                {                   
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("host", ip)
                    });

                    var result = client.PostAsync("api/oper/blacklist", formContent).Result;
                    if(result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine($"{ip} added!");
                    }
                    else
                    {
                        Console.WriteLine($"Something went wrong!");
                        Console.WriteLine(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        //public async Task RemoveBlacklist(string param)
        //{

        //    await httpClient.DeleteAsync($"api/oper/blacklist?{param}");
        //}

        public async Task<ClavisterBlacklistResponse> ListBlacklist(string param)
        {
            using (var client = CreateClient())
            {
                var blacklisted = await client.GetFromJsonAsync<ClavisterBlacklistResponse>($"api/oper/blacklist{param}");
                return blacklisted;
            }
        }
    }
}