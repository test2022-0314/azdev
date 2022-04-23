using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
var ConnectionString = config["ApplicationInsights:ConnectionString"];

using var channel = new InMemoryChannel();
var i = 5000;
while (true)
{
    i++;
    var s = $"now: {DateTime.Now}, i: {i}";
    Console.WriteLine(s);
    try
    {
        IServiceCollection services = new ServiceCollection();
        services.Configure<TelemetryConfiguration>(config =>
        {
            config.TelemetryChannel = channel;
            config.ConnectionString = ConnectionString;
        });
        services.AddLogging(builder =>
        {
            builder.AddApplicationInsights();
        });

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogTrace("trace");
        logger.LogDebug("debug");
        logger.LogInformation("info: {}", s);
        logger.LogWarning("warning!!!");
        logger.LogError("error");
        logger.LogCritical("critical");
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
    finally
    {
        // Explicitly call Flush() followed by Delay, as required in console apps.
        // This ensures that even if the application terminates, telemetry is sent to the back end.
        channel.Flush();

        await Task.Delay(TimeSpan.FromMilliseconds(10000));
    }
}
