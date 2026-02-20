namespace InHouse.BuildingBlocks.Application.Requests;

public interface IAuthorizableRequest
{
    string Policy { get; }
}
