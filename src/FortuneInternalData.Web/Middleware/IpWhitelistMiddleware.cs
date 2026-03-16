using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Web.Middleware;

public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;

    public IpWhitelistMiddleware(RequestDelegate next, ILogger<IpWhitelistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAllowedIpCacheService cacheService)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var allowed = await cacheService.IsIpAllowedAsync(ip);
        if (!allowed)
        {
            _logger.LogWarning("Blocked request from IP {Ip} — not in whitelist", ip);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("403 Forbidden — Your IP address is not allowed.");
            return;
        }

        await _next(context);
    }
}
