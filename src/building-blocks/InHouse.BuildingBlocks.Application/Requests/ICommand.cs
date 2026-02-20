namespace InHouse.BuildingBlocks.Application.Requests;

public interface ICommand<out TResponse> { }
public interface ICommand : ICommand<Primitives.Result> { }
