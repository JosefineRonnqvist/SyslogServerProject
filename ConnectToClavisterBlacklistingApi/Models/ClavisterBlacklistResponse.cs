using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectToClavisterBlacklisting.Models
{
    /// <summary>
    /// The information that Clavister-api stores
    /// </summary>
    public class BlacklistHost
    {
        public string host { get; set; }
        public string? service { get; set; }
        public int? ttl { get; set; }
        public string? alert_type { get; set; }
        public string? rule_name { get; set; }
        public string? description { get; set; }

        public override string ToString()
        {
            return $"host:{host}, service:{service}";
        }
    }

    public class ClavisterBlacklistResponse
    {
        public bool error { get; set; }
        public int blacklist_count { get; set; }
        public List<BlacklistHost>? blacklist_hosts { get; set; }
    }
}
