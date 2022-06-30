using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServerProject.Models
{
    /// <summary>
    /// Information about blacklist that is stored in database
    /// </summary>
    [Table("Blacklisted")]
    public class Blacklist
    {
        [Key]
        public int id { get; set; }
        public DateTime logDate { get; set; }
        public string host_ip { get; set; } = "";
        public Whitelist status { get; set; } = 0;  //Same as whitelisted, to disable radiobuttons for whitelist when blacklisted, since you cant change a blacklist to Clavister api
        public string service { get; set; }= "all_services";
        public double ttl { get; set; } = 300;
        public string rule_name { get; set; } = "null";
        public bool close_established { get; set; } = false;
        public string description { get; set; } = "null";
        public Whitelist whitelisted { get; set; } = Whitelist.neutral;   //Whitelisted =1, Blacklisted with ended time = neutral = 2, Blacklisted = 4
        public int? user_id { get; set; }
        public DateTime? changeDate { get; set; } = null;
        public int renewBlacklist { get; set; } = 0;  //if you want to renew blacklist =1
        public int? rep_dest_score { get; set; } 
        public string category { get; set; } = "";

    }
    public enum Whitelist
    {
        whitelisted=1,
        neutral=2,     //Blacklisted with ended time
        blacklisted=4,
    }
}
