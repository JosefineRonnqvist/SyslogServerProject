using SyslogServerProject.Models;
using SyslogServerProject.SyslogHandlers;


SendBlacklist send = new();
Blacklist blacklist = new()
{
    host_ip="1.1.11.5",
};
send.SendToBlacklist(blacklist);
send.PrintListOfBlacklist();
//Listener.SyslogReader();

