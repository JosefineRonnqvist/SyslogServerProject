using SyslogServerProject.Models;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;

namespace SyslogWorker
{
    internal class CheckValues
    {
        private readonly SendBlacklist sendBlacklist=new();
        private static Semaphore semaphore = new Semaphore(1, 1);
        private int highest_rep_dest_score = GetRepDestScore();

        public CheckValues(SendBlacklist sendBlacklist) 
        {
            this.sendBlacklist = sendBlacklist;
        }

        public CheckValues()
        {

        }
        
        /// <summary>
        /// Get the limit of allowed destscore
        /// </summary>
        /// <returns></returns>
        private static int GetRepDestScore()
        {
            try
            {
                return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("highest_rep_dest_score"));
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "CoreITClavisterSyslogService";
                    eventLog.WriteEntry("Exception when getting highest allowed rep dest score: " + ex, EventLogEntryType.Information);
                    return 100;
                }
            }
        }      
       
        /// <summary>
        /// Check if dest score is to low or if category is phishing. If they are send to Blacklist
        /// </summary>
        /// <param name="iprep_dest_score">dest score</param>
        /// <param name="categories"></param>
        /// <param name="connsrcip">ip address</param>
        /// <param name="ip">ip address</param>
        public void CheckValue(string iprep_dest_score, string categories, string connsrcip, string ip)
        {
            string dest_score = new String(iprep_dest_score.Where(char.IsDigit).ToArray());
            if (dest_score is not "")
            {
                int rep_dest_score;
                try
                {
                    rep_dest_score = Int32.Parse(dest_score);
                    if (rep_dest_score < highest_rep_dest_score || IsCategoryFishing(categories))
                    {
                        if (connsrcip is not null && connsrcip.Trim() != "")
                        {
                            NewBlacklist(connsrcip, rep_dest_score, categories);
                        }
                        if (ip is not null && ip.Trim() != "")
                        {
                            NewBlacklist(ip, rep_dest_score, categories);
                        }
                    }
                }
                catch(Exception ex)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "CoreITClavisterSyslogService";
                        eventLog.WriteEntry("Exception when parse rep dest score: " + ex, EventLogEntryType.Information);
                    }
                }                 
            }
        }

        /// <summary>
        /// Create new blacklist, use semaphore to let only one at a time work with database, to avoid multiple logs with the same ip
        /// </summary>
        /// <param name="ip"></param>
        private void NewBlacklist(string ip, int score, string category)
        {
            Blacklist blacklist = new()
            {
                host_ip = ip,
                rep_dest_score=score,
                category=category
            };
            semaphore.WaitOne();
            sendBlacklist.SendToBlacklist(blacklist);
            semaphore.Release();
        }

        /// <summary>
        /// Check if category is phishing
        /// </summary>
        /// <param name="category"></param>
        /// <returns>true if category is phishing</returns>
        private bool IsCategoryFishing(string category)
        {
            if (category.Trim().ToLower() == "phishing") { return true; }
            return false;
        }
    }
}
