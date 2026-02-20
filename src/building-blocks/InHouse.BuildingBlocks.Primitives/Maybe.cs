namespace InHouse.BuildingBlocks.Primitives;

public readonly struct Maybe<T>
{
    private readonly T? _value;

    public bool HasValue { get; }
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("No value present.");

    private Maybe(T value)
    {
        _value = value;
        HasValue = true;
    }

    public static Maybe<T> None => new();

    public static Maybe<T> From(T value)
        => value is null ? None : new(value);

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
        => HasValue ? some(_value!) : none();
}
