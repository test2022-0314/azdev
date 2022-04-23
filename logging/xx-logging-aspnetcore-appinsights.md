# プロジェクトを作成
dotnet new web
dotnet new gitignore
dotnet add package Microsoft.ApplicationInsights.AspNetCore
dotnet add package Microsoft.Extensions.Logging.ApplicationInsights

# Azureリソースを作成
project=project05
name="${project}res$(date |md5sum |head -c6)"
az group create -n $name -l japaneast

az monitor app-insights component create --app $name -l japaneast -g $name
connection_string=$(az monitor app-insights component show --app $name -g $name --query connectionString --output tsv)
echo $connection_string

# appsettings.jsonに以下を追記
```
  "ApplicationInsights": {
    "ConnectionString": "..."
  }
```

# プログラム

```c#
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
```

# 動作確認

アプリを実行。

ログがApplication Insightsに流れて表示されるようになるまで5分ほどかかる。

Application Insightsを開き、「トランザクションの検索」または「ログ」＞で「traces」や「requests」「exceptions」をクエリ。

requests: Microsoft.ApplicationInsights.AspNetCore が出力した、リクエストの記録。

traces: Log～～で出力したログ。

exception: ハンドリングされていない例外。


