using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.SuperadminOnly)]
public class PermissionsController : Controller
{
    private readonly IPermissionService _permissionService;

    private static readonly string[] Modules = { "Dashboard", "PhoneData", "Imports", "Users" };
    private static readonly string[] ManagedRoles = { Roles.Admin, Roles.Manager, Roles.Staff };

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetAllPermissionsAsync(cancellationToken);
        ViewBag.Permissions = permissions;
        ViewBag.Modules = Modules;
        ViewBag.Roles = ManagedRoles;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(IFormCollection form, CancellationToken cancellationToken)
    {
        var permissions = new List<RolePermission>();

        foreach (var role in ManagedRoles)
        {
            foreach (var module in Modules)
            {
                var canView = form[$"perm_{role}_{module}_view"] == "on";
                var canEdit = form[$"perm_{role}_{module}_edit"] == "on";

                permissions.Add(new RolePermission
                {
                    RoleId = role,
                    Module = module,
                    CanView = canView,
                    CanEdit = canEdit
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

        // Admin: all modules, view+edit
        foreach (var module in Modules)
        {
            defaults.Add(new RolePermission { RoleId = Roles.Admin, Module = module, CanView = true, CanEdit = true });
        }

        // Manager: PhoneData+Imports view+edit, Dashboard view
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "Dashboard", CanView = true, CanEdit = false });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "PhoneData", CanView = true, CanEdit = true });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "Imports", CanView = true, CanEdit = true });
        defaults.Add(new RolePermission { RoleId = Roles.Manager, Module = "Users", CanView = false, CanEdit = false });

        // Staff: PhoneData view, Dashboard view
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "Dashboard", CanView = true, CanEdit = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "PhoneData", CanView = true, CanEdit = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "Imports", CanView = false, CanEdit = false });
        defaults.Add(new RolePermission { RoleId = Roles.Staff, Module = "Users", CanView = false, CanEdit = false });

        await _permissionService.SavePermissionsAsync(defaults, cancellationToken);
        TempData["SuccessMessage"] = "Default permissions seeded successfully.";
        return RedirectToAction(nameof(Index));
    }
}
