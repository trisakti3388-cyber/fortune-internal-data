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
    public async Task<IActionResult> Create(UploadImportViewModel model, CancellationToken cancellationToken)
    {
        if (model.File is null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Please choose a file.");
            return View(model);
        }

        await using var stream = model.File.OpenReadStream();
        var storedFilePath = await _fileStorageService.SaveAsync(stream, model.File.FileName, cancellationToken);
        await _importService.ValidateUploadAsync(storedFilePath, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(ulong batchId, CancellationToken cancellationToken)
    {
        var batch = await _importQueryService.GetBatchDetailAsync(batchId, cancellationToken);
        if (batch is null)
            return NotFound();

        return View(new ImportBatchDetailViewModel { Batch = batch });
    }

    [Authorize(Policy = PolicyNames.ImportConfirm)]
    [HttpPost]
    public async Task<IActionResult> Confirm(ulong batchId, CancellationToken cancellationToken)
    {
        await _importService.ConfirmImportAsync(batchId, userId: 1, cancellationToken);
        return RedirectToAction(nameof(Detail), new { batchId });
    }
}
