namespace FortuneInternalData.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? TwoFactorSecret { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsActive { get; set; } = true;
}
