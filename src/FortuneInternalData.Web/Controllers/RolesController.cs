using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.SuperadminOnly)]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        return View(roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["ErrorMessage"] = "Role name cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        roleName = roleName.Trim();

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            TempData["ErrorMessage"] = $"Role '{roleName}' already exists.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        else
        {
            TempData["SuccessMessage"] = $"Role '{roleName}' created successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string roleName)
    {
        if (roleName == Roles.Superadmin)
        {
            TempData["ErrorMessage"] = "Cannot delete the Superadmin role.";
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        else
        {
            TempData["SuccessMessage"] = $"Role '{roleName}' deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
