namespace FortuneInternalData.Application.Interfaces;

public interface IAllowedIpCacheService
{
    Task<bool> IsIpAllowedAsync(string ip);
    void InvalidateCache();
}
