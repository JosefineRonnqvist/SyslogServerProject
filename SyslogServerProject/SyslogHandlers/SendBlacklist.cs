using Dapper;
using Dapper.Contrib.Extensions;
using SyslogServerProject.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace SyslogServerProject.SyslogHandlers
{
    internal class SendBlacklist
    {
        private readonly string ConnectionString = "Data Source=COREIT-DRIFTSIN\\SQLEXPRESS;Initial Catalog=Coreit_clavister_blacklist;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private int ttl = 300;
        private string service= "all_services";
        public SendBlacklist(){}

        /// <summary>
        /// Sends new blacklist to database, wuth todays date
        /// </summary>
        /// <param name="ip"></param>
        private void SendNewBlacklistToDB(string ip)
        {
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                Blacklist blacklist = new()
                {
                    logDate = DateTime.Now,
                    host_ip = ip,
                    ttl = ttl,
                    service = service
                };
                var id = conn.Insert(blacklist);
                Console.WriteLine("Id in DB: " + id);
            }
        }

        /// <summary>
        /// Check if ip is in database
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private bool CheckIfIpIsBlacklisted(string ip)
        {
            var query = @"SELECT host_ip FROM Blacklisted WHERE host_ip=@ip";
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                var alreadyBlacklisted = conn.QuerySingleOrDefault<Blacklist>(query, new { ip = ip });
                if (alreadyBlacklisted is not null)
                {
                    Console.WriteLine("Already in database: " + ip);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Send ip to blacklist
        /// </summary>
        /// <param name="ip">the ip address to blacklist</param>
        public void SendToBlacklist(string ip)
        {
            Console.WriteLine("Send to blacklist: " + ip);

            if (!CheckIfIpIsBlacklisted(ip))
            {
                SendNewBlacklistToDB(ip);
                SendToClavisterBlacklist(ip);
            }
        }

        private void SendToClavisterBlacklist(string ip)
        {
            var handler = new HttpClientHandler();

            handler.ClientCertificateOptions = ClientCertificateOption.Manual;

            handler.ServerCertificateCustomValidationCallback =

                (httpRequestMessage, cert, cetChain, policyErrors) =>

                {

                    return true;

                };


            using (var client = new HttpClient(handler))
            {
               
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization",
                Convert.ToBase64String(Encoding.Default.GetBytes("coreit:qQrqFuGOsHO8vmvV")));
                string param = $"host={ip}&ttl={ttl}service={service}";
                var result = client.PostAsJsonAsync(new Uri("https://81.21.224.5/api/oper/blacklist"), param).Result;

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
    }
}
