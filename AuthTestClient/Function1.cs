using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace AuthTestClient;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
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
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://authtestserver-hsgbf2b3aehadjbq.australiaeast-01.azurewebsites.net/api/Function1"),
            Headers =
    {
        { "User-Agent", "insomnia/11.4.0" },
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
}