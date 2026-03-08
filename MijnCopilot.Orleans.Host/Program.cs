using Orleans.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleans(siloBuilder =>
{
    siloBuilder
        .UseLocalhostClustering()
        .AddMemoryGrainStorageAsDefault()
        .AddDashboard();
});

var app = builder.Build();

app.MapOrleansDashboard();

await app.RunAsync();