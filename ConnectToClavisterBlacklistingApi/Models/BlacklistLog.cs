using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectToClavisterBlacklisting.Models
{
    /// <summary>
    /// Logs if the same ip is blacklisted many times
    /// </summary>
    [Table("BlacklistedLogs")]
    public class BlacklistLog
    {
        [Key]
        public int id { get; set; }
        public int blacklistedId { get; set; }
        public DateTime logDate { get; set; }
        public double ttl { get; set; } = 300;
    }
}
