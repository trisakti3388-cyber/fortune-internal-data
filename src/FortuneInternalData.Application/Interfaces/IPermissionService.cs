using FortuneInternalData.Domain.Entities;

namespace FortuneInternalData.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string roleName, string module, bool requireEdit = false, CancellationToken cancellationToken = default);
    Task<bool> HasExportPermissionAsync(string roleName, string module, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePermission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task SavePermissionsAsync(IEnumerable<RolePermission> permissions, CancellationToken cancellationToken = default);
}
