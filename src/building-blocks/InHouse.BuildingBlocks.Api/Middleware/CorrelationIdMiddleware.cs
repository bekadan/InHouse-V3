using InHouse.BuildingBlocks.Api.Http;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetOrCreate(context, InHouseHeaders.CorrelationId, create: () => Guid.CreateVersion7().ToString("N"));
        context.Items[InHouseHeaders.CorrelationId] = correlationId;

        // RequestId (gateway olabilir) yoksa TraceIdentifier ile doldur
        var requestId = GetOrCreate(context, InHouseHeaders.RequestId, create: () => context.TraceIdentifier);
        context.Items[InHouseHeaders.RequestId] = requestId;

        // Response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[InHouseHeaders.CorrelationId] = correlationId;
            context.Response.Headers[InHouseHeaders.RequestId] = requestId;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetOrCreate(HttpContext ctx, string header, Func<string> create)
    {
        if (ctx.Request.Headers.TryGetValue(header, out var v) && !string.IsNullOrWhiteSpace(v.ToString()))
            return v.ToString().Trim();

        var created = create();
        // Request header’a yazmak şart değil; ama downstream middleware’lerde kolaylık için yazıyoruz
        ctx.Request.Headers[header] = created;
        return created;
    }
}
