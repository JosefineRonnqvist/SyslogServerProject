using ConnectToClavisterBlacklisting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyslogServerProject.Models;

namespace SyslogServerProjectTests
{
    [TestClass]
    public class ToClavisterBlacklistTest
    {
        [TestMethod]
        public void SendToClavisterBlacklistReturn()
        {
            Blacklist blacklist = new() { host_ip = "1.1.1.1" };

            ToClavisterBlacklist.SendToClavisterBlacklist(blacklist);
        }
    }
}