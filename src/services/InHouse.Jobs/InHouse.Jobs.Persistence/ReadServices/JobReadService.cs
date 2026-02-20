using InHouse.BuildingBlocks.Persistence.CompiledQueries;
using InHouse.Jobs.Application.Queries;
using InHouse.Jobs.Persistence.Queries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.ReadServices;

public sealed class JobReadService : IJobReadService
{
    private readonly JobsReadDbContext _db;
    private readonly ICompiledQueryCache _compiled;

    public JobReadService(JobsReadDbContext db, ICompiledQueryCache compiled)
    {
        _db = db;
        _compiled = compiled;
    }

    public async Task<PagedResult<JobListItemDto>> ListCompanyJobsAsync(
        Guid companyId, int page, int size, CancellationToken cancellationToken = default)
    {
        if (page < 0) page = 0;
        if (size <= 0) size = 20;
        if (size > 200) size = 200;

        var skip = page * size;
        var take = size;

        var listDel = _compiled.GetOrAdd(JobsCompiledQueries.CompanyJobsListKey, JobsCompiledQueries.BuildCompanyJobsList);
        var countDel = _compiled.GetOrAdd(JobsCompiledQueries.CompanyJobsCountKey, JobsCompiledQueries.BuildCompanyJobsCount);

        var items = new List<JobListItemDto>(take);

        await foreach (var item in listDel(_db, companyId, skip, take).WithCancellation(cancellationToken))
            items.Add(item);

        var total = await countDel(_db, companyId);

        return new PagedResult<JobListItemDto>(items, total, page, size);
    }
}