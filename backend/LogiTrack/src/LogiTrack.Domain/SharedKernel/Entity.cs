namespace LogiTrack.Domain.SharedKernel;

public abstract class Entity(Guid id) : IEquatable<Entity>
{
    public Guid Id { get; protected set; } = id;

    public bool Equals(Entity? other) =>
        other is not null && GetType() == other.GetType() && Id == other.Id;

    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
