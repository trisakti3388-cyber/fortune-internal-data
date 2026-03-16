namespace FortuneInternalData.Domain.Entities;

public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
}
