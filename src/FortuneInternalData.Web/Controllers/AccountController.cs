using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

public class AccountController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ITotpSetupService _totpSetupService;

    public AccountController(IIdentityService identityService, ITotpSetupService totpSetupService)
    {
        _identityService = identityService;
        _totpSetupService = totpSetupService;
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var success = await _identityService.PasswordSignInAsync(model.Email, model.Password, model.RememberMe);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        return View(new VerifyOtpViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var success = await _identityService.VerifyTwoFactorCodeAsync(model.Code);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Invalid authentication code.");
            return View(model);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> Setup2Fa()
    {
        var model = await _totpSetupService.GenerateSetupAsync("current-user-id");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _identityService.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
