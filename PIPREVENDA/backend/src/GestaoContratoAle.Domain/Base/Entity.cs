namespace PortalAle.Domain.Base;

/// <summary>
/// Classe base que fornece identidade a todas as entidades de domínio.
/// Toda entidade concreta deve herdar desta classe. Ver DT-022.
/// </summary>
public class Entity
{
    public int Id { get; set; }

    public Entity()
    {
    }

    public Entity(int id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == 0 || other.Id == 0)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
