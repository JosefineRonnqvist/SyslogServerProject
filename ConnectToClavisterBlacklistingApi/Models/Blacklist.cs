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
        public string host_ip { get; set; }
        public int? status { get; set; } 
        public string service { get; set; }= "all_services";
        public double ttl { get; set; } = 300;
        public string rule_name { get; set; } = "null";
        public bool close_established { get; set; } = false;
        public string description { get; set; } = "null";
        public bool whitelisted { get; set; }
        public int user_id { get; set; }
        public DateTime? changeDate { get; set; }
    }
}
