using System.Security.Claims;
using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.UserManagement)]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IUserQueryService _userQueryService;

    public UsersController(IUserService userService, IUserQueryService userQueryService)
    {
        _userService = userService;
        _userQueryService = userQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new UserListViewModel
        {
            Users = await _userQueryService.GetUsersAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!Roles.All.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
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
}
