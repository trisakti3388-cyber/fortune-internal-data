using System.Security.Claims;
using FortuneInternalData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FortuneInternalData.Web.Filters;

public class MustChangePasswordFilter : IAsyncActionFilter
{
    private readonly UserManager<IdentityApplicationUser> _userManager;

    public MustChangePasswordFilter(UserManager<IdentityApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var identityUser = await _userManager.FindByIdAsync(userId);
                if (identityUser?.MustChangePassword == true)
                {
                    var controller = context.RouteData.Values["controller"]?.ToString();
                    var action = context.RouteData.Values["action"]?.ToString();

                    // Allow access to ChangePassword and Logout only
                    if (!(controller == "Account" && (action == "ChangePassword" || action == "Logout")))
                    {
                        context.Result = new RedirectToActionResult("ChangePassword", "Account", null);
                        return;
                    }
                }
            }
        }

        await next();
    }
}
