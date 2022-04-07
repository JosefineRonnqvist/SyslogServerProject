using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServerProject.Models
{
    [Table("Blacklisted")]
    public class Blacklist
    {
        [Key]
        public int id { get; set; }
        public DateTime logDate { get; set; }
        public string host_ip { get; set; }
        public int status { get; set; }
        public string service { get; set; }
        public int ttl { get; set; }
        public string rule_name { get; set; }
        public string close_established { get; set; }
        public string description { get; set; }
        public bool whitelisted { get; set; }
        public int user_id { get; set; }
        public DateTime? changeDate { get; set; }
    }
}
