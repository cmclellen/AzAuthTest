using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
