using InHouse.BuildingBlocks.Primitives;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.ProblemDetails;

public static class ErrorHttpMapper
{
    public static int MapStatus(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
