using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;

var credential = new DefaultAzureCredential();
var builder = WebApplication.CreateBuilder(args);

var endpoint = builder.Configuration["AppConfig:Endpoint"];
var sentinelKey = builder.Configuration["AppConfig:SentinelKey"];

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(endpoint), credential);
    options.ConfigureRefresh(refresh =>
    {
        refresh.Register(sentinelKey, true);
        refresh.SetCacheExpiration(TimeSpan.FromSeconds(3));
    });
});
builder.Services.Configure<Settings>(builder.Configuration.GetSection("TestApp:Settings"));

builder.Services.AddAzureAppConfiguration();
builder.Services.AddScoped<IBlobServiceClientProvider, BlobServiceClientProvider>();
// builder.Services.AddSingleton<IAppConfigurationRrefreshUtil, AppConfigurationRrefreshUtil>();

var app = builder.Build();

app.UseAzureAppConfiguration();

// var p = app.Services.GetRequiredService<IBlobServiceClientProvider>();
app.MapGet("/", () =>
{
    System.Console.WriteLine("get");
    var p = app.Services.CreateScope().ServiceProvider.GetRequiredService<IBlobServiceClientProvider>();
    System.Console.WriteLine(p.GetHashCode());
    var client = p.GetBlobServiceClient();
    return client.GetBlobContainers().Count() + " container(s)";
});

app.Run();

interface IBlobServiceClientProvider
{
    BlobServiceClient GetBlobServiceClient();
}

public class Settings
{
    public string BlobEndpoint { get; set; } = String.Empty;
}

class BlobServiceClientProvider : IBlobServiceClientProvider
{
    private IConfiguration _configuration;
    // private IAppConfigurationRrefreshUtil _refreshUtil;
    private readonly Settings _settings;
    
    public BlobServiceClientProvider(IConfiguration configuration/*, IAppConfigurationRrefreshUtil refreshUtil*/, IOptionsSnapshot<Settings> settings)
    {
        _configuration = configuration;
        // _refreshUtil = refreshUtil;
        _settings = settings.Value;
        // _logger = logger;
    }
    public BlobServiceClient GetBlobServiceClient()
    {
        // await _refreshUtil.Refresh();
        // var uri = new Uri(_configuration["blob:endpoint"]);
        Console.WriteLine($"endpoint: {_settings.BlobEndpoint}");
        var uri = new Uri(_settings.BlobEndpoint);
        return new BlobServiceClient(uri, new DefaultAzureCredential());
    }
}

// interface IAppConfigurationRrefreshUtil {
//     Task Refresh();
// }
// class AppConfigurationRrefreshUtil : IAppConfigurationRrefreshUtil {
//     private IConfigurationRefresherProvider _refresherProvider;
//     public AppConfigurationRrefreshUtil(IConfigurationRefresherProvider refresherProvider) {
//         _refresherProvider = refresherProvider;
//     }
//     public async Task Refresh() {
//         foreach (var refresher in _refresherProvider.Refreshers)
//         {
//             await refresher.RefreshAsync();
//         }
//     }

// }