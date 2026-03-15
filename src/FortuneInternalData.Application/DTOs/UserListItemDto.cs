namespace FortuneInternalData.Application.DTOs;

public class UserListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool TwoFactorEnabled { get; set; }
    public bool IsActive { get; set; }
}
