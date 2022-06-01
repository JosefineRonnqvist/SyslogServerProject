using SyslogServerProject.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServerProject.SyslogHandlers
{
    internal class CheckValues
    {
        private readonly SendBlacklist sendBlacklist=new();
        public CheckValues(SendBlacklist sendBlacklist) 
        {
            this.sendBlacklist = sendBlacklist;
        }

        public CheckValues()
        {

        }
     
        private int highest_rep_dest_score = Convert.ToInt32(ConfigurationManager.AppSettings.Get("highest_rep_dest_score"));
       
        /// <summary>
        /// Check if dest score is to low or if category is phishing. If they are send to Blacklist
        /// </summary>
        /// <param name="iprep_dest_score">dest score</param>
        /// <param name="categories"></param>
        /// <param name="connsrcip">ip address</param>
        /// <param name="ip">ip address</param>
        public void CheckValue(string iprep_dest_score, string categories, string connsrcip, string ip)
        {
            if (IsRepDestScoreToLow(iprep_dest_score) || IsCategoryFishing(categories))
            {
                if (connsrcip is not null && connsrcip.Trim() != "")
                {
                    NewBlacklist(connsrcip);
                }
                if (ip is not null && ip.Trim() != "")
                {
                    NewBlacklist(ip);
                }
            }
        }

        /// <summary>
        /// Create new blacklist
        /// </summary>
        /// <param name="ip"></param>
        private void NewBlacklist(string ip)
        {
            Blacklist blacklist = new()
            {
                host_ip = ip,
            };
            sendBlacklist.SendToBlacklist(blacklist);
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

        /// <summary>
        /// Check if dest score is to low
        /// </summary>
        /// <param name="iprep_dest_score"></param>
        /// <returns>true if dest score is to low</returns>
        private bool IsRepDestScoreToLow(string iprep_dest_score)
        {
            string dest_score = new String(iprep_dest_score.Where(char.IsDigit).ToArray());
            if (dest_score is not "")
            {
                int rep_dest_score = Int32.Parse(dest_score);
                if (rep_dest_score < highest_rep_dest_score)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
