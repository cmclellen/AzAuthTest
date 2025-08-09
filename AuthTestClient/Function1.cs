using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace AuthTestClient;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IConfiguration _configuration;
    private readonly TokenAcquirer _tokenAcquirer;

    public Function1(ILogger<Function1> logger, IConfiguration configuration, TokenAcquirer tokenAcquirer)
    {
        _logger = logger;
        _configuration = configuration;
        _tokenAcquirer = tokenAcquirer;
    }

    [Function("Function1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("Running...");
        var response = await CallApi();

        _logger.LogInformation("Craig says....{ResponseBody}", response);
        return new OkObjectResult("Welcome to client1; " + response);
    }

    private async Task<string> CallApi()
    {
        var msiEndpoint = _configuration.GetValue<string>("MSI_ENDPOINT")!;
        _logger.LogInformation("MSI_ENDPOINT set to [{MsiEndpoint}]", msiEndpoint);

        var token = await _tokenAcquirer.GetTokenAsync();

        var body = await DoTheRest(token);
        //var body = "short circuited";

        return body;
    }

    private async Task<string> DoTheRest(string token)
    {
        var baseUrl = _configuration.GetValue<string>("AUTHTESTSERVER_BASE_URL")!;
        _logger.LogInformation("Calling through to {BaseUrl}", baseUrl);
        var client = new HttpClient();
        client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{baseUrl}api/Function1")
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        return body;
    }
}