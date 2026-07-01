using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Compact;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

int delayMs = configuration.GetValue<int>("Logging:DelayMilliseconds", 5000);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(new CompactJsonFormatter(), "logs/app.json",
        rollingInterval: RollingInterval.Day,
        shared: true)
    .CreateLogger();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

Log.Information("SerilogJsonLogger started. Logging every {DelayMs}ms", delayMs);

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        await Task.Delay(delayMs, cts.Token);
        Log.Debug("Debug message from SerilogJsonLogger");
        Log.Information("Info message from SerilogJsonLogger");
        Log.Error("Error message from SerilogJsonLogger");
    }
}
catch (OperationCanceledException)
{
    Log.Information("SerilogJsonLogger shutting down");
}
finally
{
    await Log.CloseAndFlushAsync();
}
