var builder = Host.CreateApplicationBuilder(args);

builder.UseOrleans(siloBuilder =>
{
    siloBuilder
        .UseLocalhostClustering()
        .AddMemoryGrainStorageAsDefault()
        .UseDashboard(options =>
        {
            options.HostSelf = true;
            options.Port = 8080;
        });
});

await builder.Build().RunAsync();
