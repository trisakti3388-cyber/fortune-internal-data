namespace FortuneInternalData.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public ulong UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public ulong TargetId { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
}
