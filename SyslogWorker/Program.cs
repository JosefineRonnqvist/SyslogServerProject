using SyslogWorker;

IHost host = Host.CreateDefaultBuilder(args)
     .UseWindowsService(options =>
     {
         options.ServiceName = "CoreIT Clavister Syslog Service";
     })
    .ConfigureServices(services =>
    {
        services.AddSingleton<ListenerService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .Build();

await host.RunAsync();
