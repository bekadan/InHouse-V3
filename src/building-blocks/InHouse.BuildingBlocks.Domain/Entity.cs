namespace InHouse.BuildingBlocks.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(TId id)
    {
        Id = id;
    }

    // EF Core için
    protected Entity()
    {
        Id = default!;
    }

    public TId Id { get; protected set; }

    /// <summary>
    /// EF Core optimistic concurrency için. Servis katmanında configuration ile IsRowVersion yapılır.
    /// </summary>
    public byte[]? RowVersion { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Transient entity (Id default) eşitlik üretmesin
        if (IsTransient(this) || IsTransient(other)) return false;

        return GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        if (IsTransient(this))
            return base.GetHashCode();

        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !Equals(left, right);

    private static bool IsTransient(Entity<TId> entity)
        => EqualityComparer<TId>.Default.Equals(entity.Id, default!);
}
