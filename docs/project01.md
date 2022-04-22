# 概要

- アプリの種類: .NET Webアプリ (dotnet new web)
- App Service Webアプリとしてデプロイ
- App ConfigurationからBlobのエンドポイントを取得
- Blobにアクセス

# リソース

```
App Configuration
├開発者← App Configuration Data Owner
└マネージドID←App Configuration Data Reader

ストレージアカウント
├開発者←Storage Blob Data Contributor
└マネージドID←Storage Blob Data Contributor

App Serviceプラン B1
└App Serviceアプリ .NET 6
 └マネージドID
```

# Azureリソースの作成

Azure Cloud Shellで実行

```
project=project04
name="${project}res$(date |md5sum |head -c6)"
az group create -n $name -l japaneast
az appservice plan create -n $name -g $name -l japaneast --sku B1
az webapp create -n $name -g $name -p $name --runtime dotnet:6
webapp_principal_id=$(az webapp identity assign -g $name -n $name --query principalId --output tsv)
storage_account_id=$(az storage account create -n $name -g $name -l japaneast --sku Standard_LRS --query id --output tsv)
user=$(az account show --query user.name --output tsv)
role="Storage Blob Data Contributor"
az role assignment create --role "$role" --assignee $user --scope $storage_account_id
az role assignment create --role "$role" --assignee $webapp_principal_id --scope $storage_account_id

appconfig_id=$(az appconfig create -n $name -g $name -l japaneast --query id --output tsv)

role="App Configuration Data Owner"
az role assignment create --role "$role" --assignee $user --scope $appconfig_id

role="App Configuration Data Reader"
az role assignment create --role "$role" --assignee $webapp_principal_id --scope $appconfig_id
```

# プロジェクトの作成

ローカルで実行

```
mkdir ~/Documents/azdev/project04
cd ~/Documents/azdev/project04
dotnet new web
dotnet add package Azure.Identity --version 1.6.0
dotnet add package Azure.Storage.Blobs --version 12.11.0
# dotnet add package Azure.Data.AppConfiguration --version 1.2.0
dotnet add package Microsoft.Extensions.Configuration.AzureAppConfiguration --version 5.0.0
code .
```

# コード

appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "AppConfig": "https://project04res24df5b.azconfig.io"
  },
  "AllowedHosts": "*"
}

```


Program.cs
```csharp
using Azure.Identity;
using Azure.Storage.Blobs;

var credential = new DefaultAzureCredential();
var builder = WebApplication.CreateBuilder(args);

// builder.Host.ConfigureAppConfiguration(config =>
// {
//     config.AddAzureAppConfiguration(action =>
//     {
//         var endpoint = builder.Configuration.GetConnectionString("AppConfig");
//         action.Connect(new Uri(endpoint), credential);
//     });
// });

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
```

# 参考

https://github.com/Azure/AppConfiguration-DotnetProvider

https://docs.microsoft.com/ja-jp/azure/azure-app-configuration/quickstart-aspnet-core-app?tabs=core6x

https://blog.novacare.no/azure-app-configuration-using-managed-identity/

