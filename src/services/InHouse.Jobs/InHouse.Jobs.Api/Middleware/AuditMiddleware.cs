using InHouse.Jobs.Application.Auditing;
using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using InHouse.BuildingBlocks.Api.Http;
using InHouse.Jobs.Api.Auditing;
using System.Text.Json;

namespace InHouse.Jobs.Api.Middleware;

public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ITenantProvider tenant,
        ICurrentActor actor,
        IClock clock,
        IAuditLogger auditLogger)
    {
        // Endpoint metadata (attribute) okumak için next'ten önce endpoint null olabilir,
        // ama routing sonrası çalışıyorsan endpoint hazır olur.
        var endpoint = context.GetEndpoint();
        var auditAttr = endpoint?.Metadata.GetMetadata<AuditAttribute>();

        // Audit olmayan endpoint: devam
        if (auditAttr is null)
        {
            await _next(context);
            return;
        }

        var started = clock.UtcNow;
        await _next(context);

        // Tenant zorunlu ise middleware zaten durduruyor (sen eklemiştin)
        if (tenant.TenantId is null)
            return;

        var status = context.Response.StatusCode;
        var success = status < 400;

        // ResourceId: route’dan çekmeyi deneyelim (id, jobId vs)
        string? resourceId = null;
        if (context.Request.RouteValues.TryGetValue("id", out var idVal))
            resourceId = idVal?.ToString();
        else if (context.Request.RouteValues.TryGetValue("jobId", out var jobIdVal))
            resourceId = jobIdVal?.ToString();

        var correlationId = context.Request.Headers[InHouseHeaders.CorrelationId].ToString();
        if (string.IsNullOrWhiteSpace(correlationId)) correlationId = Guid.CreateVersion7().ToString("N");

        var requestId = context.Request.Headers[InHouseHeaders.RequestId].ToString();
        if (string.IsNullOrWhiteSpace(requestId)) requestId = context.TraceIdentifier;

        var ip = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();

        // Metadata minimal: method + path + status
        var metadataJson = JsonSerializer.Serialize(new
        {
            method = context.Request.Method,
            path = context.Request.Path.Value,
            statusCode = status,
            durationMs = (clock.UtcNow - started).TotalMilliseconds
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await auditLogger.LogAsync(new AuditEntry(
            TenantId: tenant.TenantId.Value,
            ActorId: actor.ActorId,
            Action: auditAttr.Action,
            Resource: auditAttr.Resource,
            ResourceId: resourceId,
            Success: success,
            OccurredOnUtc: started,
            CorrelationId: correlationId,
            RequestId: requestId,
            Ip: ip,
            UserAgent: userAgent,
            MetadataJson: metadataJson
        ));
    }
}