using ConnectToClavisterBlacklisting;
using ConnectToClavisterBlacklisting.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using SyslogServerProject.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;

namespace SyslogWorker
{
    internal class SendBlacklist
    {
        private readonly string connectionString;
        ConnectionStringSettings settings = System.Configuration.ConfigurationManager.ConnectionStrings["BlacklistDBConn"];

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
        private async Task<int> SendNewBlacklistToDB(Blacklist blacklist)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    Blacklist newBlacklist = new()
                    {
                        logDate = DateTime.Now,
                        host_ip = blacklist.host_ip,
                        whitelisted = Whitelist.blacklisted,
                        rep_dest_score = blacklist.rep_dest_score,
                        category = blacklist.category,
                    };
                    var id =await conn.InsertAsync(newBlacklist);
                    return id;
                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry($"Exception posting new blacklist in db: {ex}", EventLogEntryType.Information);
                }
                return 0;
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

            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    var alreadyBlacklisted = await conn.QueryFirstOrDefaultAsync<Blacklist>(query, new { ip = ip });
                    return alreadyBlacklisted;
                }
            }
            catch (SqlException ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception getting blacklist from db: " + ex, EventLogEntryType.Information);
                }
                return null;
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
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    var blacklistLog = await conn.QueryAsync<BlacklistLog>(query, new { blacklistedId = id });
                    return blacklistLog.ToList();
                }
            }
            catch (SqlException ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception getting blacklist log from db: " + ex, EventLogEntryType.Information);
                }
                return Enumerable.Empty<BlacklistLog>();
            }
        }

        /// <summary>
        /// Update existing blacklist with new logdate
        /// </summary>
        /// <param name="blacklistInDB">blacklist with ip blacklisted before</param>
        private void UpdateBlacklistInDB(Blacklist blacklistInDB)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    blacklistInDB.logDate = DateTime.Now;

                    var isUpdated = conn.Update(blacklistInDB);
                }
            }
            catch (SqlException ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception updating blacklist in db: " + ex, EventLogEntryType.Information);
                }
            }
        }

        /// <summary>
        /// Log this blacklist
        /// </summary>
        /// <param name="log">The blacklist</param>
        private void LogBlacklist(int id)
        {
            try
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
            catch (SqlException ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception insert blacklist log in db: " + ex, EventLogEntryType.Information);
                }
            }
        }

        /// <summary>
        /// Delete log when to many is saved
        /// </summary>
        /// <param name="id">id of logged blacklist</param>
        private void DeleteOldestBlacklist(int id)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Delete(new BlacklistLog { id = id });
                }
            }
            catch (SqlException ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception deleting blacklist from db: " + ex, EventLogEntryType.Information);
                }
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
            var blacklistAlreadyInDB = CheckIfIpIsBlacklisted(blacklist.host_ip).Result;

            if (blacklistAlreadyInDB is not null)
            {
                var blacklistLog = CheckIpBlacklistLog(blacklistAlreadyInDB.id).Result;
                if (blacklistLog is null) throw new ArgumentNullException(nameof(blacklistLog));
                var numberOfLogs = blacklistLog.Count();

                if (blacklistAlreadyInDB.whitelisted!=Whitelist.whitelisted)//Not whitelisted
                {
                    if (blacklistAlreadyInDB.logDate.AddSeconds(blacklistAlreadyInDB.ttl) < DateTime.Now)
                    {                       
                        if (numberOfLogs >= 10)
                        {
                            var id = blacklistLog.Last().id;
                            DeleteOldestBlacklist(id);
                        }
                        blacklistAlreadyInDB.whitelisted = Whitelist.blacklisted;
                        blacklistAlreadyInDB.category = blacklist.category;
                        blacklistAlreadyInDB.rep_dest_score = blacklist.rep_dest_score;
                        blacklistAlreadyInDB.logDate = DateTime.Now;
                        UpdateBlacklistInDB(blacklistAlreadyInDB);
                        CallLogAndClavisterSender(blacklistAlreadyInDB);  
                    }
                }
            }
            else if (blacklistAlreadyInDB is null)
            {               
                blacklist.id = SendNewBlacklistToDB(blacklist).GetAwaiter().GetResult();
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
            try
            {
                toClavisterBlacklist.SendToClavisterBlacklist(blacklist);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception sending blacklist to Clavister api: " + ex, EventLogEntryType.Information);
                }
            }           
        }

        /// <summary>
        /// Gets all blacklists that are meant to renew and check if time ended and renew
        /// </summary>
        public async void CheckBlacklistToRenew()
        {
            try
            {
                var query = @"SELECT id, host_ip, logDate, ttl, renewBlacklist FROM Blacklisted WHERE renewBlacklist=1";
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    var blacklistWithRenew = await conn.QueryAsync<Blacklist>(query);
                    foreach (var blacklist in blacklistWithRenew)
                    {
                        if (blacklist.logDate.AddSeconds(blacklist.ttl) < DateTime.Now)
                        {
                            SendToBlacklist(blacklist);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception getting blacklist to renew from db: " + ex, EventLogEntryType.Information);
                }
            }
        }
    }
}
