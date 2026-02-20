using System.Diagnostics.CodeAnalysis;

namespace InHouse.BuildingBlocks.Primitives;

public abstract record StronglyTypedId<TValue>
{
    public TValue Value { get; }

    protected StronglyTypedId(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        Value = value;
    }

    public override string ToString() => Value!.ToString()!;
}
