using Microsoft.Extensions.Logging;
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddFilter(nameof(Program), LogLevel.Trace);
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogTrace("trace");
logger.LogDebug("debug");
logger.LogInformation("information");
logger.LogWarning("warning");
logger.LogError("error");
logger.LogCritical("critical");
