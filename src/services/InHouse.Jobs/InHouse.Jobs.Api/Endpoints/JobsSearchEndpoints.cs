using InHouse.BuildingBlocks.Abstractions;
using InHouse.Jobs.Persistence;
using Microsoft.EntityFrameworkCore;

public static class JobsSearchEndpoints
{
    public static IEndpointRouteBuilder MapJobSearch(this IEndpointRouteBuilder app)
    {
        app.MapGet("/jobs/search", async (
            string? q,
            JobsReadDbContext db,
            CancellationToken ct) =>
        {
            var results = await db.JobList
                .FromSqlRaw(
                    @"SELECT * FROM job_list
                      WHERE search_vector @@ plainto_tsquery('english', {0})
                      ORDER BY ts_rank(search_vector, plainto_tsquery('english', {0})) DESC
                      LIMIT 20", q)
                .ToListAsync(ct);

            return Results.Ok(results);
        });

        app.MapPost("/jobs/search/semantic", async (
            string query,
            JobsReadDbContext db,
            IEmbeddingService embedding,
            CancellationToken ct) =>
        {
            var queryEmbedding = await embedding.GenerateAsync(query, ct);

            var results = await db.JobList
                .FromSqlRaw(
                    @"SELECT * FROM job_list
              ORDER BY embedding <-> {0}
              LIMIT 20", queryEmbedding)
                .ToListAsync(ct);

            return Results.Ok(results);
        });

        return app;
    }
}