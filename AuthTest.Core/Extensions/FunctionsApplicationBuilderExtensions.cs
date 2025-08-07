using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AuthTest.Core.Extensions
{
    public static class FunctionsApplicationBuilderExtensions
    {
        public static FunctionsApplicationBuilder ConfigureAuth(this FunctionsApplicationBuilder builder)
        {
            builder.UseFunctionsAuthorization();
            var cfg = builder.Configuration;
            builder.Services
                .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtFunctionsBearer(options =>
                {
                    string azureTenantIdConfigurationKey = "AZURE_TENANT_ID";
                    string apiClientIdConfigurationKey = "ApiClientId";
                    var tenantId = cfg.GetValue<string>(azureTenantIdConfigurationKey);
                    var apiClientId = cfg.GetValue<string>(apiClientIdConfigurationKey);

                    options.Authority = $"https://login.microsoftonline.com/{tenantId}";
                    options.Audience = $"api://{apiClientId}";
                });

            return builder;
        }

        public static void ConfigureLogging(this FunctionsApplicationBuilder builder) {
            // Read Application Insights connection string from configuration
            string? appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            // Configure Serilog to log to both Console and Application Insights
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(
                    appInsightsConnectionString,
                    TelemetryConverter.Traces)
                .CreateLogger();

            // Register Serilog as the logging provider
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });
        }
    }
}
