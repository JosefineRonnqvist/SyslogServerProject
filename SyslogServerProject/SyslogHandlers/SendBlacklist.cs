using Dapper;
using Dapper.Contrib.Extensions;
using SyslogServerProject.Models;
using System.Data;
using System.Data.SqlClient;


namespace SyslogServerProject.SyslogHandlers
{
    internal class SendBlacklist
    {
        private readonly string ConnectionString = "Data Source=COREIT-DRIFTSIN\\SQLEXPRESS;Initial Catalog=Coreit_clavister_blacklist;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

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
            //SendNewBlacklistToDB(ip);

            if (!CheckIfIpIsBlacklisted(ip))
            {
                SendNewBlacklistToDB(ip);
            }
        }

        //private void SendToBlacklist(string ip)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        //BlacklistModel blacklist_model = new() { host = ip, ttl = time };
        //        client.BaseAddress = new Uri("");
        //        string param = $"host={ip}&ttl={ttl}";
        //        var response = client.PostAsJsonAsync("api/oper/blacklist", param).Result;
        //        if (response.IsSuccessStatusCode)
        //        {
        //            Console.Write("Success");
        //        }
        //        else
        //        {
        //            Console.Write("Error");
        //        }
        //    }
        //}
    }
}
