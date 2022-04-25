using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectToClavisterBlacklisting.Models
{
    [Table("BlacklistedLogs")]
    public class BlacklistLog
    {
        [Key]
        public int id { get; set; }
        public int blacklistedId { get; set; }
        public DateTime logDate { get; set; }
        public int? ttl { get; set; }
    }
}
