using ConnectToClavisterBlacklisting;
using ConnectToClavisterBlacklisting.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using SyslogServerProject.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SyslogServerProject.SyslogHandlers
{
    internal class SendBlacklist
    {

        private readonly string connectionString;
        ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["BlacklistDBConn"];

        /// <summary>
        /// Uses Connectionsettings to get connectionstring from app.config
        /// </summary>
        public SendBlacklist()
        {
            this.connectionString = settings.ConnectionString;
        }

        /// <summary>
        /// Sends new blacklist to database, with todays date
        /// </summary>
        /// <param name="ip">ip to blacklist</param>
        private int SendNewBlacklistToDB(string ip)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                Blacklist blacklist = new()
                {
                    logDate = DateTime.Now,
                    host_ip = ip,
                };
                var id = (int)conn.Insert(blacklist);
                Console.WriteLine("Id in DB: " + id);
                return id;
            }
        }

        /// <summary>
        /// Check if ip is in database
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>The blacklist in db, or null</returns>
        private async Task<Blacklist?> CheckIfIpIsBlacklisted(string ip)
        {
            var query = @"SELECT id, host_ip, whitelisted FROM Blacklisted WHERE host_ip=@ip";
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
        /// Gets the log of the blacklisted ip
        /// </summary>
        /// <param name="id">id of blacklisted ip</param>
        /// <returns>list of the logs of this ip</returns>
        private async Task<IEnumerable<BlacklistLog?>> CheckIpBlacklistLog(int id)
        {
            var query = @"SELECT id, blacklistedId, logDate, ttl 
                        FROM BlacklistedLogs
                        WHERE blacklistedId=@blacklistedId
                        ORDER BY id";
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                var blacklistLog = await conn.QueryAsync<BlacklistLog>(query, new { blacklistedId = id });
                return blacklistLog.ToList();
            }
        }

        /// <summary>
        /// Update existing blacklist with new logdate
        /// </summary>
        /// <param name="blacklistInDB">blacklist with ip blacklisted before</param>
        private void UpdateBlacklistInDB(Blacklist blacklistInDB)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                blacklistInDB.logDate = DateTime.Now;

                var id = conn.Update(blacklistInDB);
            }
        }

        /// <summary>
        /// Log this blacklist
        /// </summary>
        /// <param name="log">The blacklist</param>
        private void LogBlacklist(int id)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                var newLog = new BlacklistLog
                {
                    blacklistedId = id,
                    logDate = DateTime.Now,
                };
                conn.Insert(newLog);
            }
        }

        /// <summary>
        /// Delete log when to many is saved
        /// </summary>
        /// <param name="id">id of logged blacklist</param>
        private void DeleteOldestBlacklist(int id)
        {
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                conn.Delete(new BlacklistLog { id = id });
            }
        }

        /// <summary>
        /// Send ip to blacklist.
        /// Check if blacklisted before, if time to live ended and if whitelisted.
        /// Log the blacklist, if more then 10 is logged delete oldest. 
        /// </summary>
        /// <param name="ip">the ip address to blacklist</param>
        public void SendToBlacklist(Blacklist blacklist)
        {
            Console.WriteLine("Send to blacklist: " + blacklist.host_ip);
            var alreadyInDB = CheckIfIpIsBlacklisted(blacklist.host_ip).Result;

            if (alreadyInDB is not null)
            {
                var blacklistLog = CheckIpBlacklistLog(alreadyInDB.id).Result;
                if (blacklistLog is null) throw new ArgumentNullException(nameof(blacklistLog));
                var numberOfLogs = blacklistLog.Count();

                if (1<alreadyInDB.whitelisted)
                {
                    if (alreadyInDB.logDate.AddSeconds(alreadyInDB.ttl) < DateTime.Now)
                    {
                        if (numberOfLogs >= 10)
                        {
                            var id = blacklistLog.Last().id;
                            DeleteOldestBlacklist(id);
                        }
                        UpdateBlacklistInDB(alreadyInDB);
                        CallLogAndClavisterSender(alreadyInDB);
                        Console.WriteLine($"{blacklist.host_ip} has been blacklisted before");
                    }
                    else
                    {
                        Console.WriteLine($"{blacklist.host_ip} is already blacklisted");
                    }
                }
                else
                {
                    Console.WriteLine($"{blacklist.host_ip} is whitelisted");
                }
            }
            else if (alreadyInDB is null)
            {               
                blacklist.id = SendNewBlacklistToDB(blacklist.host_ip);
                CallLogAndClavisterSender(blacklist);
            }
        }

        /// <summary>
        /// Log the blacklist and send to Clavister
        /// </summary>
        /// <param name="id"></param>
        /// <param name="blacklist"></param>
        private void CallLogAndClavisterSender(Blacklist blacklist)
        {
            LogBlacklist(blacklist.id);
            ToClavisterBlacklist toClavisterBlacklist = new();
            toClavisterBlacklist.SendToClavisterBlacklist(blacklist);
        }

        /// <summary>
        /// Get list of ip blacklisted by Clavister api
        /// </summary>
        public void PrintListOfBlacklist()
        {
            ToClavisterBlacklist toClavisterBlacklist = new();
            var blacklistedList = toClavisterBlacklist.ListBlacklist().Result;
            if (blacklistedList is null) throw new ArgumentNullException(nameof(blacklistedList));

            foreach (var blacklisted in blacklistedList.blacklist_hosts)
            {
                Console.WriteLine($"Found in Clavister Blacklist: {blacklisted}");
            }
        }
    }
}
