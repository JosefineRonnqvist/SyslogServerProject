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
            var connect = new ToClavisterBlacklist();
            Blacklist blacklist = new() { host_ip = "1.1.1.1" };

            connect.SendToClavisterBlacklist(blacklist);
        }
    }
}