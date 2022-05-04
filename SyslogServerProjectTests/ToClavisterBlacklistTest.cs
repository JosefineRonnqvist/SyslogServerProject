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
            ToClavisterBlacklist toClavisterBlacklist = new();
            Blacklist blacklist = new() { host_ip = "1.1.1.1" };

            var result =toClavisterBlacklist.SendToClavisterBlacklist(blacklist);
            Assert.AreEqual(result.StatusCode, "200");
        }
    }
}