using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthTestClient;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IConfiguration configuration;

    public Function1(ILogger<Function1> logger, IConfiguration configuration)
    {
        _logger = logger;
        this.configuration = configuration;
    }

    [Function("Function1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("Running...");
        var response = await CallApi();

        _logger.LogInformation("Craig says....C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to client1; " + response);
    }

    private async Task<string> CallApi()
    {
        var token = await GetToken();

        var baseUrl = configuration.GetValue<string>("AUTHTESTSERVER_BASE_URL")!;
        _logger.LogInformation("Calling through to {BaseUrl}", baseUrl);
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{baseUrl}api/Function1"),
            Headers =
            {
                { "Authorization", $"Bearer ${token}" }
            }
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);
        return body;
    }

    private async Task<string> GetToken()
    {
        _logger.LogInformation("Obtaining token...");

        ManagedIdentityCredential miCredential = new();
        var token = await miCredential.GetTokenAsync(GetTokenRequestContext());
        _logger.LogInformation("Token obtained {Token}", token.Token);
        return token.Token;
    }

    //private TokenRequestContext GetTokenRequestContext()
    //{
    //    var miAudience = "api://AzureADTokenExchange";
    //    TokenRequestContext tokenRequestContext = new([$"{miAudience}/.default"]);
    //    return tokenRequestContext;
    //}

    private TokenRequestContext GetTokenRequestContext()
    {
        try
        {
            var miAudience = configuration.GetValue<string>("MI_AUDIENCE")!;
            var tokenRequestContextUri = $"{miAudience}/.default";
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
}