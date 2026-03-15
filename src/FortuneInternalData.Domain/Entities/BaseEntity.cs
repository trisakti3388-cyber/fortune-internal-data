namespace FortuneInternalData.Domain.Entities;

public abstract class BaseEntity
{
    public ulong Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
