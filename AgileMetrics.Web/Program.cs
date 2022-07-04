using AgileMetrics.Core.APIs.AzureDevOps;
using AgileMetrics.Core.Builds;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

var config = app.Configuration.GetSection("BuildDefinitions")
    .Get<List<AzureDevopsApiConfig>>();

// Map a route to get all build definitions
app.MapGet("/api/builds", () => config.Select(c => new
{
    c.DisplayName,
    c.BuildDefinition
}));

// Map a route for each build definition's metrics
foreach (var apiConfig in config)
{
    var metrics = new BuildMetricsGenerator(new AzureDevopsApi(apiConfig));
    app.MapGet($"/api/builds/{apiConfig.BuildDefinition}/metrics",
        async ([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        {
            var results = await metrics.GetBuildMetrics(from, to);
            return results;
        });
}

app.Run();