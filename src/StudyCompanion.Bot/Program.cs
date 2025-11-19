using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudyCompanion.Bot;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure services
builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("BotConfiguration"));

builder.Services.AddHostedService<BotService>();

var host = builder.Build();

await host.RunAsync();
