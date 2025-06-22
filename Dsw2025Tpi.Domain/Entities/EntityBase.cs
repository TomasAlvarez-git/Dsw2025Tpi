namespace Dsw2025Tpi.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; }
    protected EntityBase()
    {
        Id = Guid.NewGuid();
    }

}
