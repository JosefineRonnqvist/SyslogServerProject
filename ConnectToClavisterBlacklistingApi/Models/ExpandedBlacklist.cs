using SyslogServerProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectToClavisterBlacklisting.Models
{
    public class ExpandedBlacklist
    {
        public Blacklist? blacklist { get; set; } = null;
        public IEnumerable<BlacklistLog?>? blacklistLogs { get; set; } = null;
        public bool showLog { get; set; } = false;
    }
}
