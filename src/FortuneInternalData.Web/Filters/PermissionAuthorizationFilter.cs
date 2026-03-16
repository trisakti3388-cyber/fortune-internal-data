using System.Security.Claims;
using FortuneInternalData.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FortuneInternalData.Web.Filters;

public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionService _permissionService;

    private static readonly Dictionary<string, string> ControllerModuleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dashboard"] = "Dashboard",
        ["PhoneNumbers"] = "PhoneData",
        ["Imports"] = "Imports",
        ["Users"] = "Users",
        ["Permissions"] = "Users",
        ["WebSummary"] = "WebData",
        ["IpWhitelist"] = "IpWhitelist",
        ["Roles"] = "Users"
    };

    public PermissionAuthorizationFilter(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip if user is not authenticated (handled by [Authorize])
        if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            return;

        var controllerName = context.RouteData.Values["controller"]?.ToString();
        if (controllerName == null || !ControllerModuleMap.TryGetValue(controllerName, out var module))
            return;

        // Get role from claims (ASP.NET Identity stores roles as claims at login)
        var roleName = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleName))
            return;

        var isPost = context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        var hasPermission = await _permissionService.HasPermissionAsync(roleName, module, isPost);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
