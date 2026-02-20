using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Api.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InHouse.BuildingBlocks.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context, IEventContextAccessor eventContextAccessor)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            if (context.Response.HasStarted)
                throw;

            var pd = ProblemDetailsFactory.FromException(context, ex, eventContextAccessor);

            context.Response.Clear();
            context.Response.StatusCode = pd.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await JsonSerializer.SerializeAsync(context.Response.Body, pd,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            await context.Response.CompleteAsync();
        }
    }
}
