using MediatR;

namespace InHouse.Jobs.Application.Queries.ListCompanyJobs;

public sealed record ListCompanyJobsQuery(Guid CompanyId, int Page, int Size)
    : IRequest<PagedResult<JobListItemDto>>;