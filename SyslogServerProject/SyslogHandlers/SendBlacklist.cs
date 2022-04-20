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

        private readonly string connectionString;
        ConnectionStringSettings settings =
         ConfigurationManager.ConnectionStrings["BlacklistDBConn"];

        /// <summary>
        /// Uses Connectionsettings to get connectionstring from app.config
        /// </summary>
        public SendBlacklist()
        {
            this.connectionString = settings.ConnectionString;
        }

        /// <summary>
        /// Sends new blacklist to database, wuth todays date
        /// </summary>
        /// <param name="ip"></param>
        private async Task SendNewBlacklistToDB(string ip)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                Blacklist blacklist = new()
                {
                    logDate = DateTime.Now,
                    host_ip = ip,
                };
                var id = await conn.InsertAsync(blacklist);
                Console.WriteLine("Id in DB: " + id);
            }
        }

        /// <summary>
        /// Check if ip is in database
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private async Task<Blacklist?> CheckIfIpIsBlacklisted(string ip)
        {
            var query = @"SELECT host_ip, whitelisted FROM Blacklisted WHERE host_ip=@ip";
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                var alreadyBlacklisted = await conn.QuerySingleOrDefaultAsync<Blacklist>(query, new { ip = ip });
                if (alreadyBlacklisted is not null)
                {
                    Console.WriteLine("Already in database: " + ip);
                }
                return alreadyBlacklisted;
            }
        }

        /// <summary>
        /// Update existing blacklist with new logdate
        /// </summary>
        /// <param name="blacklistInDB">blacklist with ip blacklisted before</param>
        /// <returns></returns>
        private async Task UpdateBlacklistInDB(Blacklist blacklistInDB)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                blacklistInDB.logDate = DateTime.Now;
               
                var id = await conn.UpdateAsync(blacklistInDB);
            }
        }

        /// <summary>
        /// Send ip to blacklist
        /// </summary>
        /// <param name="ip">the ip address to blacklist</param>
        public async Task SendToBlacklist(string ip)
        {
            Console.WriteLine("Send to blacklist: " + ip);
            var alreadyInDB = CheckIfIpIsBlacklisted(ip).Result;

            if(alreadyInDB is not null && !alreadyInDB.whitelisted)
            {        
                await UpdateBlacklistInDB(alreadyInDB);      
                ToClavisterBlacklist sender = new ();
                sender.SendToClavisterBlacklist(ip);
            }
            else if (alreadyInDB is null)
            {
                await SendNewBlacklistToDB(ip);
                ToClavisterBlacklist sender = new();
                sender.SendToClavisterBlacklist(ip);
            }
        }

        /// <summary>
        /// Get list of ip blacklisted by Clavister api
        /// </summary>
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
    }
}
