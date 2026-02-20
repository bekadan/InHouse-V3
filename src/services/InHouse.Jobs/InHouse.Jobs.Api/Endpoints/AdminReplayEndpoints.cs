using InHouse.BuildingBlocks.Abstractions.Integration.Replay;

namespace InHouse.Jobs.Api.Endpoints;

public static class AdminReplayEndpoints
{
    public static IEndpointRouteBuilder MapAdminReplayEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/admin/replay/dlq", async (
            EventReplayRequest request,
            IEventReplayService replayer,
            CancellationToken ct) =>
        {
            // burada normalde: auth + policy check (platform admin)
            var result = await replayer.ReplayFromDlqAsync(request, ct);
            return Results.Ok(result);
        })
        // .RequireAuthorization("PlatformAdmin")  // sizde policy adı neyse
        ;

        return endpoints;
    }
}