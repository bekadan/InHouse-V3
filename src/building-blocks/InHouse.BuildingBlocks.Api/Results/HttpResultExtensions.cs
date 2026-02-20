using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Api.ProblemDetails;
using InHouse.BuildingBlocks.Primitives;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.Results;

public static class HttpResultExtensions
{
    public static IResult ToHttpResult(this Result result, HttpContext httpContext, IEventContextAccessor ctx)
    {
        if (result.IsSuccess)
            return Microsoft.AspNetCore.Http.Results.NoContent();

        var pd = ProblemDetailsFactory.FromResult(httpContext, result, ctx);
        return Microsoft.AspNetCore.Http.Results.Problem(
            title: pd.Title,
            detail: pd.Detail,
            statusCode: pd.Status,
            instance: pd.Instance,
            extensions: pd.Extensions);
    }

    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext httpContext, IEventContextAccessor ctx)
    {
        if (result.IsSuccess)
            return Microsoft.AspNetCore.Http.Results.Ok(result.Value);

        var pd = ProblemDetailsFactory.FromResult(httpContext, result, ctx);
        return Microsoft.AspNetCore.Http.Results.Problem(
            title: pd.Title,
            detail: pd.Detail,
            statusCode: pd.Status,
            instance: pd.Instance,
            extensions: pd.Extensions);
    }
}
