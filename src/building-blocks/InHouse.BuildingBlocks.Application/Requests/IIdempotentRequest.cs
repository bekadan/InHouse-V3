namespace InHouse.BuildingBlocks.Application.Requests;

public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}
