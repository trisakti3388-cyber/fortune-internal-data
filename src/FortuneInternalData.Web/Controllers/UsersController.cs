using System.Security.Claims;
using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.UserManagement)]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IUserQueryService _userQueryService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(IUserService userService, IUserQueryService userQueryService, RoleManager<IdentityRole> roleManager)
    {
        _userService = userService;
        _userQueryService = userQueryService;
        _roleManager = roleManager;
    }

    private async Task<string[]> GetAllRolesAsync()
    {
        return await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToArrayAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new UserListViewModel
        {
            Users = await _userQueryService.GetUsersAsync(cancellationToken)
        };

        ViewBag.AllRoles = await GetAllRolesAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.AllRoles = await GetAllRolesAsync();
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = await GetAllRolesAsync();
            return View(model);
        }

        var allRoles = await GetAllRolesAsync();
        if (!allRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
            ViewBag.AllRoles = allRoles;
            return View(model);
        }

        var (success, errors) = await _userService.CreateAsync(new CreateUserDto
        {
            Name = model.Name,
            Email = model.Email,
            Role = model.Role,
            Password = model.Password
        }, cancellationToken);

        if (!success)
        {
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);
            ViewBag.AllRoles = allRoles;
            return View(model);
        }

        TempData["SuccessMessage"] = $"User '{model.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string userId, CancellationToken cancellationToken)
    {
        var (success, errors) = await _userService.ToggleActiveAsync(userId, cancellationToken);
        if (!success)
            TempData["ErrorMessage"] = string.Join("; ", errors);
        else
            TempData["SuccessMessage"] = "User status updated.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        var (success, errors) = await _userService.ResetPasswordAsync(model.UserId, model.NewPassword, cancellationToken);
        if (!success)
            TempData["ErrorMessage"] = string.Join("; ", errors);
        else
            TempData["SuccessMessage"] = $"Password for '{model.UserName}' has been reset. They will be prompted to change it on next login.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model, CancellationToken cancellationToken)
    {
        var allRoles = await GetAllRolesAsync();
        if (!allRoles.Contains(model.NewRole))
        {
            TempData["ErrorMessage"] = "Invalid role selected.";
            return RedirectToAction(nameof(Index));
        }

        var (success, errors) = await _userService.ChangeRoleAsync(model.UserId, model.NewRole, cancellationToken);
        if (!success)
            TempData["ErrorMessage"] = string.Join("; ", errors);
        else
            TempData["SuccessMessage"] = $"Role for '{model.UserName}' changed to '{model.NewRole}'.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(string userId, CancellationToken cancellationToken)
    {
        var (success, errors) = await _userService.UnlockUserAsync(userId, cancellationToken);
        if (!success)
            TempData["ErrorMessage"] = string.Join("; ", errors);
        else
            TempData["SuccessMessage"] = "User account unlocked successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset2Fa(string userId, CancellationToken cancellationToken)
    {
        var (success, errors) = await _userService.Reset2FaAsync(userId, cancellationToken);
        if (!success)
            TempData["ErrorMessage"] = string.Join("; ", errors);
        else
            TempData["SuccessMessage"] = "Two-factor authentication has been reset. The user will need to set up 2FA again.";

        return RedirectToAction(nameof(Index));
    }
}
