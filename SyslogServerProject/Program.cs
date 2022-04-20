using SyslogServerProject.SyslogHandlers;


SendBlacklist send = new();
send.SendToBlacklist("123.34.44.3.9");
//send.PrintListOfBlacklist();
//Listener.SyslogReader();

