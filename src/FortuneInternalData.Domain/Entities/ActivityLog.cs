namespace FortuneInternalData.Domain.Entities;

public class ActivityLog
{
    public ulong Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public ulong TargetId { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
