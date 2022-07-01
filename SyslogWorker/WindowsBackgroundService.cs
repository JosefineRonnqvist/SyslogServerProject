namespace SyslogWorker
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly ListenerService _listenerService;
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(        
            ListenerService listenerService,
            ILogger<WindowsBackgroundService>logger) =>
            (_listenerService, _logger) = (listenerService, logger);
        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _listenerService.SyslogReader();
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
            }
        }
    }
}