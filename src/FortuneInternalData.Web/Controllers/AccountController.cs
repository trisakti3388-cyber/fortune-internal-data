using System.Security.Claims;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Infrastructure.Identity;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

public class AccountController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ITotpSetupService _totpSetupService;
    private readonly IUserService _userService;
    private readonly UserManager<IdentityApplicationUser> _userManager;

    public AccountController(
        IIdentityService identityService,
        ITotpSetupService totpSetupService,
        IUserService userService,
        UserManager<IdentityApplicationUser> userManager)
    {
        _identityService = identityService;
        _totpSetupService = totpSetupService;
        _userService = userService;
        _userManager = userManager;
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

    /// <summary>
    /// Forced password change (no current password required) - shown when MustChangePassword = true.
    /// </summary>
    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction(nameof(Login));

        var (success, errors) = await _userService.ResetPasswordAsync(userId, model.NewPassword);
        if (!success)
        {
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        // Clear the MustChangePassword flag
        await _userService.SetMustChangePasswordAsync(userId, false);

        TempData["SuccessMessage"] = "Your password has been changed successfully.";
        return RedirectToAction("Index", "Dashboard");
    }

    /// <summary>
    /// Voluntary password change (requires current password).
    /// </summary>
    [Authorize]
    [HttpGet]
    public IActionResult ChangeOwnPassword()
    {
        return View(new ChangeOwnPasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeOwnPassword(ChangeOwnPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction(nameof(Login));

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["SuccessMessage"] = "Your password has been changed successfully.";
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
