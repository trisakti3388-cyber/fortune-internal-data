using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _dbContext;

    public PermissionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasPermissionAsync(string roleName, string module, bool requireEdit = false, CancellationToken cancellationToken = default)
    {
        // Superadmin always has full access
        if (roleName == Roles.Superadmin)
            return true;

        var permission = await _dbContext.RolePermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.RoleId == roleName && p.Module == module, cancellationToken);

        if (permission == null)
            return false;

        return requireEdit ? permission.CanEdit : permission.CanView;
    }

    public async Task<bool> HasExportPermissionAsync(string roleName, string module, CancellationToken cancellationToken = default)
    {
        if (roleName == Roles.Superadmin)
            return true;

        var permission = await _dbContext.RolePermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.RoleId == roleName && p.Module == module, cancellationToken);

        return permission?.CanExport ?? false;
    }

    public async Task<IReadOnlyList<RolePermission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.RolePermissions
            .AsNoTracking()
            .OrderBy(p => p.RoleId)
            .ThenBy(p => p.Module)
            .ToListAsync(cancellationToken);
    }

    public async Task SavePermissionsAsync(IEnumerable<RolePermission> permissions, CancellationToken cancellationToken = default)
    {
        var permList = permissions.ToList();

        // Delete all existing and re-insert
        await _dbContext.RolePermissions.ExecuteDeleteAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var p in permList)
        {
            p.CreatedAt = now;
            p.UpdatedAt = now;
        }

        await _dbContext.RolePermissions.AddRangeAsync(permList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
