using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FortuneInternalData.Infrastructure.Services;

public class AllowedIpCacheService : IAllowedIpCacheService
{
    private static IReadOnlySet<string>? _cachedIps;
    private static DateTime _lastRefresh = DateTime.MinValue;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;

    public AllowedIpCacheService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> IsIpAllowedAsync(string ip)
    {
        // Always allow localhost
        if (ip is "127.0.0.1" or "::1" or "localhost" or "::ffff:127.0.0.1")
            return true;

        var ips = await GetCachedIpsAsync();

        // If table is empty, allow all
        if (ips.Count == 0) return true;

        return ips.Contains(ip);
    }

    public void InvalidateCache()
    {
        _cachedIps = null;
        _lastRefresh = DateTime.MinValue;
    }

    private async Task<IReadOnlySet<string>> GetCachedIpsAsync()
    {
        if (_cachedIps != null && DateTime.UtcNow - _lastRefresh < CacheDuration)
            return _cachedIps;

        await _lock.WaitAsync();
        try
        {
            if (_cachedIps != null && DateTime.UtcNow - _lastRefresh < CacheDuration)
                return _cachedIps;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ips = await dbContext.AllowedIps.Select(x => x.IpAddress).ToListAsync();
            _cachedIps = new HashSet<string>(ips, StringComparer.OrdinalIgnoreCase);
            _lastRefresh = DateTime.UtcNow;
            return _cachedIps;
        }
        finally
        {
            _lock.Release();
        }
    }
}
