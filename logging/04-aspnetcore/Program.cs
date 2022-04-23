var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration["ApplicationInsights:ConnectionString"];
Console.WriteLine(cs);
builder.Services.AddApplicationInsightsTelemetry(cs);
builder.Services.AddLogging(builder =>
{
    builder.AddApplicationInsights();
});
var app = builder.Build();
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
var i = 0;
app.MapGet("/", () =>
{
    i++;
    var s = $"now: {DateTime.Now}, i: {i}";
    logger.LogTrace("trace");
    logger.LogDebug("debug");
    logger.LogInformation("info: {}", s);
    logger.LogWarning("warning!!!");
    logger.LogError("error");
    logger.LogCritical("critical");
    if (i % 10 == 0)
    {
        throw new Exception("test!");
    }
    return s;
});

app.Run();
