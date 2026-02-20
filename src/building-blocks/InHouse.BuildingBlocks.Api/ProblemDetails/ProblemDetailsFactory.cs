using System.Collections.Generic;
using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Primitives;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.ProblemDetails;

public static class ProblemDetailsFactory
{
    private static void AddExtensions(
        Microsoft.AspNetCore.Mvc.ProblemDetails pd,
        EventContext ctx,
        IEnumerable<object> errors = null)
    {
        // Use a custom dictionary to store extensions
        pd.Type = pd.Type ?? string.Empty;
        pd.Title = pd.Title ?? string.Empty;
        pd.Instance = pd.Instance ?? string.Empty;

        // Add extension values as JSON string in Detail or Instance, or use a custom property if you have a derived ProblemDetails type
        var extensionValues = new Dictionary<string, object>
        {
            ["correlationId"] = ctx.CorrelationId,
            ["requestId"] = ctx.RequestId,
            ["tenantId"] = ctx.TenantId
        };

        if (errors != null)
        {
            extensionValues["errors"] = errors;
        }

        // Serialize extensionValues and append to Detail
        pd.Detail += $" | Extensions: {System.Text.Json.JsonSerializer.Serialize(extensionValues)}";
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails FromResult(
        HttpContext httpContext,
        Result result,
        IEventContextAccessor eventContextAccessor)
    {
        var ctx = eventContextAccessor.Current;

        var errors = result.Errors.Count == 0
            ? new[] { DomainErrorCatalog.General.Unexpected }
            : result.Errors;

        // status: en “ağır” olanı seç (unauth/forbidden/notfound vs)
        var status = errors
            .Select(e => ErrorHttpMapper.MapStatus(e.Type))
            .DefaultIfEmpty(StatusCodes.Status500InternalServerError)
            .Max();

        var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                _ => "Server Error"
            },
            Detail = errors.Count == 1 ? errors[0].Message : "One or more errors occurred.",
            Instance = httpContext.Request.Path
        };

        AddExtensions(pd, ctx, errors.Select(e => new { e.Code, e.Message, type = e.Type.ToString() }).ToArray());

        return pd;
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails FromException(
        HttpContext httpContext,
        Exception ex,
        IEventContextAccessor eventContextAccessor)
    {
        var ctx = eventContextAccessor.Current;

        var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };

        AddExtensions(pd, ctx);

        // prod’da exception detayını basma; dev ortamında ayrıca eklenebilir
        return pd;
    }
}
