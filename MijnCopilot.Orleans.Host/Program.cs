using Azure.Storage.Blobs;
using Orleans.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleans(siloBuilder =>
{
    siloBuilder
        .UseLocalhostClustering()
        .AddMemoryGrainStorageAsDefault()
        .AddAzureBlobGrainStorage("blob-store", options =>
        {
            options.BlobServiceClient = new BlobServiceClient(
                builder.Configuration.GetValue<string>("BLOB_CONNECTION_STRING")!);
        })
        .AddDashboard();
});

var app = builder.Build();

app.MapOrleansDashboard();

await app.RunAsync();