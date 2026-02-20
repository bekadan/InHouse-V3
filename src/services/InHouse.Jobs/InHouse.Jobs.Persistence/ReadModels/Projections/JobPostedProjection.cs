using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Projection;
using InHouse.BuildingBlocks.Persistence.ReadModels;
using InHouse.Jobs.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public sealed class JobPostedProjection : IProjectionHandler
{
    private readonly JobsReadDbContext _db;
    private readonly IEmbeddingService _embedding;

    public JobPostedProjection(JobsReadDbContext db, IEmbeddingService embedding)
    {
        _db = db;
        _embedding = embedding;
    }

    public string EventType => "Jobs.JobPosted";
    public int EventVersion => 2;

    public async Task ProjectAsync(IntegrationEventEnvelope envelope, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<JobPostedPayload>(envelope.PayloadJson)!;

        var textForEmbedding = $"{payload.Title} {payload.Description}";
        var embedding = await _embedding.GenerateAsync(textForEmbedding, ct);

        var existing = await _db.JobList.FindAsync(new object[] { payload.JobId }, ct);

        if (existing is null)
        {
            existing = new JobListItem
            {
                Id = payload.JobId,
                TenantId = envelope.TenantId,
                CompanyId = payload.CompanyId,
                Title = payload.Title,
                Description = payload.Description,
                EmploymentType = payload.EmploymentType,
                Status = "Published",
                CreatedOnUtc = envelope.OccurredOnUtc
            };

            _db.JobList.Add(existing);
        }
        else
        {
            existing.Title = payload.Title;
            existing.Description = payload.Description;
            existing.EmploymentType = payload.EmploymentType;
        }

        existing.Embedding = embedding;

        // FTS vector update
        await _db.Database.ExecuteSqlRawAsync(
            @"UPDATE job_list 
              SET search_vector = to_tsvector('english', title || ' ' || description)
              WHERE id = {0}", payload.JobId);

        await _db.SaveChangesAsync(ct);
    }

    private sealed record JobPostedPayload(
        Guid JobId,
        Guid CompanyId,
        string Title,
        string Description,
        string EmploymentType);
}