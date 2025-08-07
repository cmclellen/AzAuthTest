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
        var response = await CallApi();

        _logger.LogInformation("Craig says....C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to client1; " + response);
    }

    private async Task<string> CallApi()
    {
        var token = await GetToken();

        string baseUrl = configuration.GetValue<string>("AUTHTESTSERVER_BASE_URL")!;
        _logger.LogInformation("Calling through to " + baseUrl);
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{baseUrl}api/Function1"),
            Headers =
    {
        { "User-Agent", "insomnia/11.4.0" },
          {"Authorization", $"Bearer ${token}" }
    },
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);
            return body;
        }
    }

    private async Task<string> GetToken()
    {
        ManagedIdentityCredential miCredential = new ();
        string miAudience = "api://AzureADTokenExchange";
        TokenRequestContext tokenRequestContext = new([$"{miAudience}/.default"]);
        var token = await miCredential.GetTokenAsync(tokenRequestContext);

        _logger.LogInformation("Token: " + token);
        return token.Token;

    }
}