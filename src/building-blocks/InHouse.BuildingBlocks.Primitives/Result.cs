namespace InHouse.BuildingBlocks.Primitives;

public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }

    public static Result Success()
        => new(true, []);

    public static Result Failure(Error error)
        => new(false, [error]);

    public static Result Failure(IEnumerable<Error> errors)
        => new(false, errors.ToList());
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(true, [])
    {
        _value = value;
    }

    private Result(IReadOnlyList<Error> errors)
        : base(false, errors)
    {
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result.");

    public static Result<T> Success(T value)
        => new(value);

    public static new Result<T> Failure(Error error)
        => new([error]);

    public static new Result<T> Failure(IEnumerable<Error> errors)
        => new(errors.ToList());
}
