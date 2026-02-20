using InHouse.Jobs.Application.Abstractions;
using InHouse.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly JobsDbContext _db;

    public JobRepository(JobsDbContext db)
    {
        _db = db;
    }

    public async Task<Job?> GetByIdAsync(Guid jobId, CancellationToken ct)
    {
        return await _db.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, ct);
    }

    public async Task AddAsync(Job job, CancellationToken ct)
    {
        await _db.Jobs.AddAsync(job, ct);
        await _db.SaveChangesAsync(ct);
    }
}