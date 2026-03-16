using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.SuperadminOnly)]
public class PermissionsController : Controller
{
    private readonly IPermissionService _permissionService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public static readonly string[] Modules = { "Dashboard", "PhoneData", "Imports", "Users", "WebData", "IpWhitelist", "PhoneAssign" };

    public PermissionsController(IPermissionService permissionService, RoleManager<IdentityRole> roleManager)
    {
        _permissionService = permissionService;
        _roleManager = roleManager;
    }

    private async Task<string[]> GetManagedRolesAsync()
    {
        var allRoles = await _roleManager.Roles
            .Where(r => r.Name != Roles.Superadmin)
            .Select(r => r.Name!)
            .OrderBy(r => r)
            .ToListAsync();
        return allRoles.ToArray();
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetAllPermissionsAsync(cancellationToken);
        ViewBag.Permissions = permissions;
        ViewBag.Modules = Modules;
        ViewBag.Roles = await GetManagedRolesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(IFormCollection form, CancellationToken cancellationToken)
    {
        var managedRoles = await GetManagedRolesAsync();
        var permissions = new List<RolePermission>();

        foreach (var role in managedRoles)
        {
            foreach (var module in Modules)
            {
                var canView = form[$"perm_{role}_{module}_view"] == "on";
                var canEdit = form[$"perm_{role}_{module}_edit"] == "on";
                var canExport = form[$"perm_{role}_{module}_export"] == "on";

                permissions.Add(new RolePermission
                {
                    RoleId = role,
                    Module = module,
                    CanView = canView,
                    CanEdit = canEdit,
                    CanExport = canExport
                });
            }
        }

        await _permissionService.SavePermissionsAsync(permissions, cancellationToken);
        TempData["SuccessMessage"] = "Permissions saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
    {
        var defaults = new List<RolePermission>();

        // Admin: all modules, view+edit+export
        foreach (var module in Modules)
        {
            defaults.Add(new RolePermission { RoleId = Roles.Admin, Module = module, CanView = true, CanEdit = true, CanExport = true });
        }

        // Manager: PhoneData+Imports+WebData view+edit+export, Dashboard view
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "Dashboard", CanView = true, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "PhoneData", CanView = true, CanEdit = true, CanExport = true });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "Imports", CanView = true, CanEdit = true, CanExport = true });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "Users", CanView = false, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "WebData", CanView = true, CanEdit = true, CanExport = true });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "IpWhitelist", CanView = false, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "PhoneAssign", CanView = true, CanEdit = false, CanExport = false });

        // Staff: PhoneData view, Dashboard view
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "Dashboard", CanView = true, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "PhoneData", CanView = true, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "Imports", CanView = false, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "Users", CanView = false, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "WebData", CanView = false, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "IpWhitelist", CanView = false, CanEdit = false, CanExport = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "PhoneAssign", CanView = false, CanEdit = false, CanExport = false });

        await _permissionService.SavePermissionsAsync(defaults, cancellationToken);
        TempData["SuccessMessage"] = "Default permissions seeded successfully.";
        return RedirectToAction(nameof(Index));
    }
}
