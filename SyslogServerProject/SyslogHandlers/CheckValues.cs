using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServerProject.SyslogHandlers
{
    internal class CheckValues
    {
        private readonly SendBlacklist sendBlacklist;
        public CheckValues(SendBlacklist sendBlacklist) 
        {
            this.sendBlacklist = sendBlacklist;
        }

        public CheckValues()
        {

        }
     
        private const int highest_rep_dest_score = 20;
       
        public void CheckValue(string iprep_dest_score, string categories, string connsrcip, string ip)
        {
            if (IsRepDestScoreToLow(iprep_dest_score) || IsCategoryFishing(categories))
            {
                if (connsrcip is not null && connsrcip.Trim() != "")
                {
                    sendBlacklist.SendToBlacklist(connsrcip);
                }
                if (ip is not null && ip.Trim() != "")
                {
                    sendBlacklist.SendToBlacklist(ip);
                }
            }
        }

        private bool IsCategoryFishing(string category)
        {
            if (category.Trim().ToLower() == "phishing") { return true; }
            return false;
        }
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
