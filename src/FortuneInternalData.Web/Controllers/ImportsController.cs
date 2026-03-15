using System.Security.Claims;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.ManagerOrAbove)]
public class ImportsController : Controller
{
    private readonly IImportService _importService;
    private readonly IImportQueryService _importQueryService;
    private readonly IFileStorageService _fileStorageService;

    public ImportsController(
        IImportService importService,
        IImportQueryService importQueryService,
        IFileStorageService fileStorageService)
    {
        _importService = importService;
        _importQueryService = importQueryService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new ImportHistoryViewModel
        {
            Items = await _importQueryService.GetRecentBatchesAsync(cancellationToken)
        };

        return View(model);
    }

    [Authorize(Policy = PolicyNames.AdminOrAbove)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new UploadImportViewModel());
    }

    [Authorize(Policy = PolicyNames.AdminOrAbove)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UploadImportViewModel model, CancellationToken cancellationToken)
    {
        if (model.File is null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Please choose a file.");
            return View(model);
        }

        var ext = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (ext != ".csv" && ext != ".xlsx")
        {
            ModelState.AddModelError(nameof(model.File), "Only CSV and XLSX files are supported.");
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        await using var stream = model.File.OpenReadStream();
        var storedFilePath = await _fileStorageService.SaveAsync(stream, model.File.FileName, cancellationToken);
        var batchId = await _importService.ValidateAndCreateBatchAsync(storedFilePath, model.File.FileName, userId, cancellationToken);

        return RedirectToAction(nameof(Detail), new { id = batchId });
    }

    [HttpGet]
    public async Task<IActionResult> Detail(ulong id, CancellationToken cancellationToken)
    {
        var batch = await _importQueryService.GetBatchDetailAsync(id, cancellationToken);
        if (batch is null)
            return NotFound();

        return View(new ImportBatchDetailViewModel { Batch = batch });
    }

    [Authorize(Policy = PolicyNames.AdminOrAbove)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ulong id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await _importService.ConfirmImportAsync(id, userId, cancellationToken);

        TempData["SuccessMessage"] = "Import confirmed. New records have been inserted.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = PolicyNames.AdminOrAbove)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(ulong id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await _importService.CancelImportAsync(id, userId, cancellationToken);

        TempData["SuccessMessage"] = "Import has been cancelled.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}
