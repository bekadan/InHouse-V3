using InHouse.BuildingBlocks.Persistence.CompiledQueries;
using InHouse.Jobs.Application.Queries;
using InHouse.Jobs.Domain;
using InHouse.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Queries;

public static class JobsCompiledQueries
{
    public static readonly string CompanyJobsListKey =
        CompiledQueryKeys.For<JobsReadDbContext>("CompanyJobsList_v1");

    public static readonly string CompanyJobsCountKey =
        CompiledQueryKeys.For<JobsReadDbContext>("CompanyJobsCount_v1");

    public static Func<JobsReadDbContext, Guid, int, int, IAsyncEnumerable<JobListItemDto>>
        BuildCompanyJobsList()
        => EF.CompileAsyncQuery(
            (JobsReadDbContext db, Guid companyId, int skip, int take) =>
                db.Set<Job>()
                  .Where(j => j.CompanyId == companyId)
                  .OrderByDescending(j => j.CreatedOnUtc)
                  .Skip(skip).Take(take)
                  .Select(j => new JobListItemDto(j.Id, j.Title, j.Status, j.CreatedOnUtc)));

    public static Func<JobsReadDbContext, Guid, Task<int>>
        BuildCompanyJobsCount()
        => EF.CompileAsyncQuery(
            (JobsReadDbContext db, Guid companyId) =>
                db.Set<Job>().Count(j => j.CompanyId == companyId));
}