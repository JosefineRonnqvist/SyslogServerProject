using SyslogServerProject.Models;
using SyslogServerProject.SyslogHandlers;


SendBlacklist send = new();
Blacklist blacklist = new()
{
    host_ip="1.1.1.2",
};
send.SendToBlacklist(blacklist);
send.PrintListOfBlacklist();
//Listener.SyslogReader();

