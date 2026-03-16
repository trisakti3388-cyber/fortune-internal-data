namespace FortuneInternalData.Domain.Entities;

public class AllowedIp : BaseEntity
{
    public string IpAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
}
