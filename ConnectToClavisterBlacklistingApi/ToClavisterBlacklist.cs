using SyslogServerProject.Models;
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

        public async Task SendToClavisterBlacklist(string ip)
        {


            using (var client = CreateClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization",
                Convert.ToBase64String(Encoding.Default.GetBytes("coreit:qQrqFuGOsHO8vmvV")));
                string param = $"host={ip}&ttl={ttl}service={service}";
                var result = await client.PostAsJsonAsync(new Uri("https://81.21.224.5/api/oper/blacklist"), param);

                //string param = $"host={ip}&ttl={ttl}service={service}";
                //WebRequest req = WebRequest.Create(@"https://81.21.224.5/" + param);
                //req.Method = "POST";
                //req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("coreit:qQrqFuGOsHO8vmvV"));
                ////req.Credentials = new NetworkCredential("username", "password");
                //HttpWebResponse resp = req.GetResponse() as HttpWebResponse;

                //client.BaseAddress = new Uri("https://81.21.224.5/");

                //var response = client.PostAsJsonAsync("api/oper/blacklist", param).Result;

                //if (response.IsSuccessStatusCode)
                //{
                //    Console.Write("Success");
                //}
                //else
                //{
                //    Console.Write("Error");
                //}
            }
        }
        //public async Task RemoveBlacklist(string param)
        //{
        //    await httpClient.DeleteAsync($"api/oper/blacklist?{param}");
        //}

        public async Task<IEnumerable<Blacklist?>> ListBlacklist(string param)
        {
            using (var client = CreateClient())
            {
                var blacklisted =  await client.GetFromJsonAsync<Blacklist[]>($"api/oper/blacklist{param}");
                return blacklisted;
            }
        }
    }
}