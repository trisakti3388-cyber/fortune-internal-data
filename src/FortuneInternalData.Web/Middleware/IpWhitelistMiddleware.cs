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
        var remoteIp = context.Connection.RemoteIpAddress;
        var ip = remoteIp?.ToString() ?? "unknown";

        // Normalize IPv4-mapped IPv6 addresses (::ffff:1.2.3.4 -> 1.2.3.4)
        if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            ip = remoteIp.MapToIPv4().ToString();

        var allowed = await cacheService.IsIpAllowedAsync(ip);
        if (!allowed)
        {
            _logger.LogWarning("Blocked request from IP {Ip} — not in whitelist", ip);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body><h1>403 Forbidden</h1><p>Your IP address (" + ip + ") is not whitelisted.</p></body></html>");
            return;
        }

        await _next(context);
    }
}
