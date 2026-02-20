using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Api;
using InHouse.BuildingBlocks.Messaging.Kafka;
using InHouse.BuildingBlocks.Persistence;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using InHouse.Jobs.Api;
using InHouse.Jobs.Api.Endpoints;
using InHouse.Jobs.Api.Middleware;
using InHouse.Jobs.Api.Security;
using InHouse.Jobs.Api.Tenancy;
using InHouse.Jobs.Persistence;
using InHouse.Jobs.Persistence.Outbox;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddJobsPersistence(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddJobsPersistence(
    writeConnectionString: builder.Configuration.GetConnectionString("JobsWrite")!);

builder.Services.AddInHouseBuildingBlocksPersistence<JobsDbContext>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

builder.Services.AddScoped<ICurrentActor, HttpCurrentActor>();

builder.Services.AddHealthChecks()
    .AddCheck<OutboxHealthCheck>("outbox");

builder.Services.AddSingleton<OutboxGaugeProvider>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService("InHouse.Jobs.Api"))
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("InHouse.Jobs.Outbox")
            .AddPrometheusExporter();
    });

builder.Services
    .AddIntegrationVersioning()
    .AddJobsEventUpcasters()
    .AddJobsIntegrationHandlers();

// Kafka messaging (infra)
builder.Services.AddKafkaMessaging(builder.Configuration);

// Inbox runner infra (senin mevcut)
builder.Services.AddIntegrationConsumerInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<AuditMiddleware>();

// routingden sonra endpointten önce tenant kontrolü yapacağız, böylece tenant header'ı olmayan istekler daha erken dönecek ve gereksiz işlemler yapılmamış olacak
app.UseMiddleware<TenantRequiredMiddleware>();

app.MapHealthChecks("/healthz");

app.Services.GetRequiredService<OutboxGaugeProvider>();

app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");

app.MapAdminReplayEndpoints();

app.UseHttpsRedirection();

app.Run();

/*
 * 
 * Admin Endpoint Örneği
app.MapGet("/api/admin/jobs/all",
async (InHouse.Jobs.Persistence.JobsReadDbContext db,
       InHouse.BuildingBlocks.Persistence.SoftDelete.ISoftDeleteFilterProvider softDelete) =>
{
    using (softDelete.BeginBypassScope())
    {
        var jobs = await db.Jobs
            .OrderByDescending(x => x.CreatedOnUtc)
            .Take(100)
            .ToListAsync();

        return Results.Ok(jobs);
    }
});


app.MapGet("/api/jobs/{id:guid}", handler).WithMetadata(new InHouse.Jobs.Api.Auditing.AuditAttribute("View", "Job"));
*/

/*
await _mediator.Send(new ApplyJobPostedIntegrationEventCommand(
    payload.JobId,
    payload.CompanyId,
    payload.Title,
    envelope.Headers?.TryGetValue("actor-id", out var actor)
        == true ? actor : "system"
), ct); */

/*
 * Replay nasıl kullanılır?
 {
  "tenantId": "tenant-123",
  "eventType": "Jobs.JobPosted",
  "eventVersion": 2,
  "occurredFromUtc": "2026-02-01T00:00:00Z",
  "occurredToUtc": "2026-02-20T00:00:00Z",
  "forceReprocess": true,
  "requestedBy": "burak.emre",
  "reason": "Projection rebuild after schema fix"
}
 
 */