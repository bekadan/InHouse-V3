using MediatR;
using InHouse.Jobs.Application.Queries;

namespace InHouse.Jobs.Application.Queries.ListCompanyJobs;

public sealed class ListCompanyJobsHandler : IRequestHandler<ListCompanyJobsQuery, PagedResult<JobListItemDto>>
{
    private readonly IJobReadService _read;

    public ListCompanyJobsHandler(IJobReadService read) => _read = read;

    public Task<PagedResult<JobListItemDto>> Handle(ListCompanyJobsQuery request, CancellationToken cancellationToken)
        => _read.ListCompanyJobsAsync(request.CompanyId, request.Page, request.Size, cancellationToken);
}