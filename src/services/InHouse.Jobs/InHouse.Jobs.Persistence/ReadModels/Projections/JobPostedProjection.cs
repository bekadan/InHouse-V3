using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Projection;
using InHouse.BuildingBlocks.Persistence.ReadModels;
using InHouse.Jobs.Persistence;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public sealed class JobPostedProjection : IProjectionHandler
{
    private readonly JobsReadDbContext _db;

    public JobPostedProjection(JobsReadDbContext db)
    {
        _db = db;
    }

    public string EventType => "Jobs.JobPosted";
    public int EventVersion => 2;

    public async Task ProjectAsync(IntegrationEventEnvelope envelope, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<JobPostedV2Payload>(envelope.PayloadJson)!;

        var existing = await _db.JobList.FindAsync(new object[] { payload.JobId }, ct);

        if (existing is null)
        {
            _db.JobList.Add(new JobListItem
            {
                Id = payload.JobId,
                CompanyId = payload.CompanyId,
                Title = payload.Title,
                EmploymentType = payload.EmploymentType,
                Status = "Published",
                CreatedOnUtc = envelope.OccurredOnUtc
            });
        }
        else
        {
            existing.Title = payload.Title;
            existing.EmploymentType = payload.EmploymentType;
        }

        await _db.SaveChangesAsync(ct);
    }

    private sealed record JobPostedV2Payload(
        Guid JobId,
        Guid CompanyId,
        string Title,
        string EmploymentType);
}