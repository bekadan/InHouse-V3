namespace InHouse.Jobs.Application.Queries;

public sealed record JobListItemDto(
    Guid Id,
    string Title,
    string Status,
    DateTime CreatedOnUtc);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int Size);

public interface IJobReadService
{
    Task<PagedResult<JobListItemDto>> ListCompanyJobsAsync(
        Guid companyId,
        int page,
        int size,
        CancellationToken cancellationToken = default);
}