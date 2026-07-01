using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

int delayMs = configuration.GetValue<int>("Logging:DelayMilliseconds", 5000);

var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()!);
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

var log = LogManager.GetLogger(typeof(Program));

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

log.Info($"Log4NetXmlLogger started. Logging every {delayMs}ms");

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        await Task.Delay(delayMs, cts.Token);
        log.Debug("Debug message from Log4NetXmlLogger");
        log.Info("Info message from Log4NetXmlLogger");
        log.Error("Error message from Log4NetXmlLogger");
    }
}
catch (OperationCanceledException)
{
    log.Info("Log4NetXmlLogger shutting down");
}
finally
{
    LogManager.Shutdown();
}
