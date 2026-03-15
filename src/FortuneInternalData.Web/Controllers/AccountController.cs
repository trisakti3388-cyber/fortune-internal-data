using System.Security.Claims;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _identityService.PasswordSignInAsync(model.Email, model.Password, model.RememberMe);

        switch (result)
        {
            case SignInResultType.Success:
                return RedirectToLocal(returnUrl);

            case SignInResultType.RequiresTwoFactor:
                return RedirectToAction(nameof(VerifyOtp), new { returnUrl });

            case SignInResultType.LockedOut:
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                return View(model);

            default:
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> VerifyOtp(string? returnUrl = null)
    {
        var userId = await _identityService.GetTwoFactorUserIdAsync();
        if (userId == null)
            return RedirectToAction(nameof(Login));

        ViewData["ReturnUrl"] = returnUrl;
        return View(new VerifyOtpViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _identityService.VerifyTwoFactorCodeAsync(model.Code);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Invalid authentication code.");
            return View(model);
        }

        return RedirectToLocal(returnUrl);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Setup2Fa()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction(nameof(Login));

        var result = await _totpSetupService.GenerateSetupAsync(userId);
        return View(new Setup2FaViewModel
        {
            ManualEntryKey = result.ManualEntryKey,
            QrCodeDataUri = result.QrCodeDataUri,
            QrCodeUri = result.QrCodeUri
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup2Fa(Setup2FaViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            var result = await _totpSetupService.GenerateSetupAsync(userId);
            model.ManualEntryKey = result.ManualEntryKey;
            model.QrCodeDataUri = result.QrCodeDataUri;
            model.QrCodeUri = result.QrCodeUri;
            return View(model);
        }

        var success = await _totpSetupService.VerifyAndEnableAsync(userId, model.Code);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");
            var result = await _totpSetupService.GenerateSetupAsync(userId);
            model.ManualEntryKey = result.ManualEntryKey;
            model.QrCodeDataUri = result.QrCodeDataUri;
            model.QrCodeUri = result.QrCodeUri;
            return View(model);
        }

        TempData["SuccessMessage"] = "Two-factor authentication has been enabled successfully.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View("~/Views/Shared/AccessDenied.cshtml");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _identityService.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard");
    }
}
