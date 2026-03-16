using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Persistence;
using FortuneInternalData.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.SuperadminOnly)]
[Route("Admin/IpWhitelist")]
public class IpWhitelistController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAllowedIpCacheService _cacheService;

    public IpWhitelistController(ApplicationDbContext dbContext, IAllowedIpCacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var ips = await _dbContext.AllowedIps
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return View("~/Views/Admin/IpWhitelist/Index.cshtml", ips);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string ipAddress, string? description, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            TempData["ErrorMessage"] = "IP address is required.";
            return RedirectToAction(nameof(Index));
        }

        ipAddress = ipAddress.Trim();

        var exists = await _dbContext.AllowedIps
            .AnyAsync(x => x.IpAddress == ipAddress, cancellationToken);

        if (exists)
        {
            TempData["ErrorMessage"] = $"IP {ipAddress} already exists.";
            return RedirectToAction(nameof(Index));
        }

        var entry = new AllowedIp
        {
            IpAddress = ipAddress,
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.AllowedIps.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _cacheService.InvalidateCache();

        TempData["SuccessMessage"] = $"IP {ipAddress} added to whitelist.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.AllowedIps.FindAsync(new object[] { id }, cancellationToken);
        if (entry is null)
        {
            TempData["ErrorMessage"] = "IP entry not found.";
            return RedirectToAction(nameof(Index));
        }

        _dbContext.AllowedIps.Remove(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _cacheService.InvalidateCache();

        TempData["SuccessMessage"] = $"IP {entry.IpAddress} removed from whitelist.";
        return RedirectToAction(nameof(Index));
    }
}
