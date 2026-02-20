using InHouse.BuildingBlocks.Primitives;

namespace InHouse.BuildingBlocks.Application.Abstractions;

public interface IExceptionToErrorMapper
{
    Error Map(Exception exception);
}
