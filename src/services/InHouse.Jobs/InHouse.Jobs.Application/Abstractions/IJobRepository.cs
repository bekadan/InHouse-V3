using InHouse.Jobs.Domain.Entities;

namespace InHouse.Jobs.Application.Abstractions;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid jobId, CancellationToken ct);

    Task AddAsync(Job job, CancellationToken ct);
}