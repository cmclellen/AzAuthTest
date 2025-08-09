using Azure.Core;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AuthTestServer;

[FunctionAuthorize]
public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Authorize(Roles = "Task.Write")]
    [Function("Function1")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request {UserIsNull}.", req.HttpContext.User == null);

        var user = req.HttpContext.User;
        var claims = user?.Claims ?? [];

        _logger.LogInformation("Claim count: {ClaimCount}; {IsInTaskWriterRole}", claims.Count(), user?.IsInRole("Task.Write"));
        
        foreach (Claim claim in claims)
        {
            var isRoleClaim = claim.Type.EndsWith("/identity/claims/role");
            if (isRoleClaim)
            {
                _logger.LogInformation($"CLAIM TYPE: {claim.Type}; CLAIM VALUE: {claim.Value}");
            }
        }

        return new OkObjectResult("Welcome to server1");
    }
}