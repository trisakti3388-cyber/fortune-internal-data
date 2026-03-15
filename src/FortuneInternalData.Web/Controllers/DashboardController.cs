using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.ManagerOrAbove)]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _dashboardService.GetSummaryAsync(cancellationToken);
        return View(model);
    }
}
