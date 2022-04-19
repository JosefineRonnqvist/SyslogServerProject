using SyslogServerProject.SyslogHandlers;

SendBlacklist send = new();
//send.PrintListOfBlacklistAsString();
send.SendToBlacklist("123.34.44.45");
send.PrintListOfBlacklist();
//Listener.SyslogReader();
