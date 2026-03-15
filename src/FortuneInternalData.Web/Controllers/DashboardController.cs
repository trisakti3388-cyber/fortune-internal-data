using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IImportQueryService _importQueryService;

    public DashboardController(IDashboardService dashboardService, IImportQueryService importQueryService)
    {
        _dashboardService = dashboardService;
        _importQueryService = importQueryService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        var recentBatches = await _importQueryService.GetRecentBatchesAsync(cancellationToken);

        ViewBag.RecentBatches = recentBatches.Take(5).ToList();
        return View(summary);
    }
}
