using System.Security.Claims;
using ClosedXML.Excel;
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
    private readonly IImportBackgroundQueue _backgroundQueue;

    public ImportsController(
        IImportService importService,
        IImportQueryService importQueryService,
        IFileStorageService fileStorageService,
        IImportBackgroundQueue backgroundQueue)
    {
        _importService = importService;
        _importQueryService = importQueryService;
        _fileStorageService = fileStorageService;
        _backgroundQueue = backgroundQueue;
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
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Template");

        worksheet.Cell(1, 1).Value = "seq";
        worksheet.Cell(1, 2).Value = "phone_number";
        worksheet.Cell(1, 3).Value = "remark";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "phone_import_template.xlsx");
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

        // Create the batch record immediately (status: "processing") and return
        var batchId = await _importService.CreatePendingBatchAsync(storedFilePath, model.File.FileName, userId, cancellationToken);

        // Queue for background processing
        _backgroundQueue.Enqueue(batchId);

        return RedirectToAction(nameof(Detail), new { id = batchId });
    }

    [HttpGet]
    public async Task<IActionResult> Detail(
        ulong id,
        int page = 1,
        int pageSize = 100,
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var batch = await _importQueryService.GetBatchDetailAsync(id, page, pageSize, statusFilter, cancellationToken);
        if (batch is null)
            return NotFound();

        return View(new ImportBatchDetailViewModel { Batch = batch });
    }

    [HttpGet]
    public async Task<IActionResult> Status(ulong id, CancellationToken cancellationToken)
    {
        var status = await _importQueryService.GetBatchStatusAsync(id, cancellationToken);
        if (status is null)
            return NotFound();

        return Json(status);
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
