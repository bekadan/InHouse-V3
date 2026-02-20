namespace InHouse.BuildingBlocks.Persistence.Idempotency;

public sealed class IdempotencyRecord
{
    public string Key { get; private set; } = default!;
    public DateTime CreatedOnUtc { get; private set; }

    private IdempotencyRecord() { } // EF

    public IdempotencyRecord(string key, DateTime createdOnUtc)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key is required.", nameof(key));

        Key = key.Trim();
        CreatedOnUtc = createdOnUtc;
    }
}
