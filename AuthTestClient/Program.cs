using AuthTest.Core.Extensions;
using AuthTestClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// Add environment variables as a configuration source
builder.Configuration.AddEnvironmentVariables();
builder.ConfigureLogging();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddTransient<TokenAcquirer>();

builder.Build().Run();
