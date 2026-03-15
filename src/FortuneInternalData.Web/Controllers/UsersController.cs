using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
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
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userService.CreateAsync(new CreateUserDto
        {
            Name = model.Name,
            Email = model.Email,
            Role = model.Role,
            Password = model.Password
        }, createdByUserId: 1, cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}
