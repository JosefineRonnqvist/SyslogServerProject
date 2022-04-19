using ConnectToClavisterBlacklisting.Models;
using SyslogServerProject.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ConnectToClavisterBlacklisting
{
    public class ToClavisterBlacklist
    {
        public ToClavisterBlacklist()
        {

        }
        private int ttl = 300;
        private string service = "all_services";

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
            return client;
        }

        public  void SendToClavisterBlacklist(string ip)
        {
            using (var client = CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.Default.GetBytes("coreit:qQrqFuGOsHO8vmvV")));
                client.BaseAddress = new Uri("https://81.21.224.5/");
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.Default.GetBytes("coreit:qQrqFuGOsHO8vmvV")));
                client.BaseAddress = new Uri("https://81.21.224.5/");
                var blacklisted = await client.GetFromJsonAsync<ClavisterBlacklistResponse>($"api/oper/blacklist{param}");
                return blacklisted;
            }
        }


        public async Task<string> ListBlacklistAsString(string param)
        {
            using (var client = CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.Default.GetBytes("coreit:qQrqFuGOsHO8vmvV")));
                client.BaseAddress = new Uri("https://81.21.224.5/");
                var blacklisted = await client.GetStringAsync($"api/oper/blacklist{param}");
                return blacklisted;
            }
        }
    }
}