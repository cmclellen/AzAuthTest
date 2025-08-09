using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace AuthTestClient
{

    public class TokenAcquirer
    {
        private readonly ILogger<TokenAcquirer> _logger;
        private readonly IConfiguration _configuration;

        public TokenAcquirer(ILogger<TokenAcquirer> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GetTokenAsync()
        {
            _logger.LogInformation("Obtaining token...");
            //DefaultAzureCredential credential = new();
            ManagedIdentityCredential credential = new();
            var token = await credential.GetTokenAsync(GetTokenRequestContext());
            _logger.LogInformation("Token obtained {Token}", token.Token);
            var tokenValue = token.Token;

            var appServerId = _configuration.GetValue<string>("SERVER_CLIENT_ID");
            var appClientId = _configuration.GetValue<string>("CLIENT_CLIENT_ID");
            var clientId = appClientId;
            _logger.LogInformation("KEY SET = {KeySet}", clientId == appClientId? "CLIENT_CLIENT_ID": "SERVER_CLIENT_ID");
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithTenantId(_configuration.GetValue<string>("AZURE_TENANT_ID"))
                .WithClientAssertion(token.Token).Build();

            
            var scope = $"api://{appServerId}/.default";
            var token2 = await app.AcquireTokenForClient([scope]).ExecuteAsync();
            _logger.LogInformation("Token 2 [{Token2}]", token2.AccessToken);
            tokenValue = token2.AccessToken;

            return tokenValue;
        }

        private TokenRequestContext GetTokenRequestContext()
        {
            try
            {
                var tokenRequestContextUri = _configuration.GetValue<string>("MI_AUDIENCE")!;
                // https://management.azure.com for the SMI token
                // api://AzureADTokenExchange/.default
                _logger.LogInformation("Constructed token request context [{TokenRequestContextUri}]", tokenRequestContextUri);
                TokenRequestContext tokenRequestContext = new([tokenRequestContextUri]);
                return tokenRequestContext;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed obtaining access token.");
                throw;
            }
        }


        //private async Task<string> GetToken1()
        //{
        //    _logger.LogInformation("Obtaining token...");

        //    ManagedIdentityCredential miCredential = new();
        //    var token = await miCredential.GetTokenAsync(GetTokenRequestContext());
        //    _logger.LogInformation("Token obtained {Token}", token.Token);
        //    return token.Token;
        //}

        //private TokenRequestContext GetTokenRequestContext()
        //{
        //    var miAudience = "api://AzureADTokenExchange";
        //    TokenRequestContext tokenRequestContext = new([$"{miAudience}/.default"]);
        //    return tokenRequestContext;
        //}
    }
}
