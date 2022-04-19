using ConnectToClavisterBlacklisting;
using ConnectToClavisterBlacklisting.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using SyslogServerProject.Models;
using System.Configuration;
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
        //private readonly string ConnectionString = "Data Source=COREIT-DRIFTSIN\\SQLEXPRESS;Initial Catalog=Coreit_clavister_blacklist;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        //private readonly IConfiguration _configuration;
        private readonly string connectionString;
        ConnectionStringSettings settings =
         ConfigurationManager.ConnectionStrings["BlacklistDBConn"];

        /// <summary>
        /// Uses IConfiguration to get the connectionstring from appsettings.json
        /// </summary>
        /// <param name="config">configuration</param>
        public SendBlacklist()
        {
            this.connectionString = settings.ConnectionString;
        }

        //public SendBlacklist() { }

        /// <summary>
        /// Sends new blacklist to database, wuth todays date
        /// </summary>
        /// <param name="ip"></param>
        private void SendNewBlacklistToDB(string ip)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
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
            using (IDbConnection conn = new SqlConnection(connectionString))
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
                //SendNewBlacklistToDB(ip);
                ToClavisterBlacklist sender = new ();
                sender.SendToClavisterBlacklist(ip);
            }
        }

        public void PrintListOfBlacklist()
        {
            ToClavisterBlacklist sender = new();
            string param = "";
            ClavisterBlacklistResponse blacklistedList = sender.ListBlacklist(param).Result;
            if(blacklistedList is not null)
            {
                 foreach (var blacklisted in blacklistedList.blacklist_hosts)
                 {
                     Console.WriteLine($"Found in Clavister Blacklist: {blacklisted}");
                 }
            }
           
        }

        public void PrintListOfBlacklistAsString()
        {
            ToClavisterBlacklist sender = new();
            string param = "";
            string blacklistedList = sender.ListBlacklistAsString(param).Result;
            if (blacklistedList is not null)
            {

                Console.WriteLine($"Found in Clavister Blacklist:{ blacklistedList}");
                
            }

        }
    }
}
