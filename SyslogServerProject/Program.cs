using SyslogServerProject.SyslogHandlers;


SendBlacklist send = new();
send.SendToBlacklist("123.34.44.2.2");
//send.PrintListOfBlacklist();
//Listener.SyslogReader();

