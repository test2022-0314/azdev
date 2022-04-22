using Azure.Identity;
using Azure.Storage.Blobs;

var credential = new DefaultAzureCredential();
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(action => {
    var endpoint = builder.Configuration.GetConnectionString("AppConfig");
    action.Connect(new Uri(endpoint), credential);
});

var app = builder.Build();

var config = app.Services.GetRequiredService<IConfiguration>();

app.MapGet("/", () =>
{
    var uri = new Uri(config["blob:uri"]);
    var client = new BlobServiceClient(uri, credential);
    return client.GetBlobContainers().Count() + " container(s)";
});

app.Run();
