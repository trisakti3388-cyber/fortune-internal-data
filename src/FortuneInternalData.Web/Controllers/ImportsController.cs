using System.Security.Claims;
using ClosedXML.Excel;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize]
public class ImportsController : Controller
{
    private readonly IImportService _importService;
    private readonly IImportQueryService _importQueryService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IImportBackgroundQueue _backgroundQueue;
    private readonly IPermissionService _permissionService;

    public ImportsController(
        IImportService importService,
        IImportQueryService importQueryService,
        IFileStorageService fileStorageService,
        IImportBackgroundQueue backgroundQueue,
        IPermissionService permissionService)
    {
        _importService = importService;
        _importQueryService = importQueryService;
        _fileStorageService = fileStorageService;
        _backgroundQueue = backgroundQueue;
        _permissionService = permissionService;
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

    [HttpGet]
    public IActionResult Create()
    {
        return View(new UploadImportViewModel());
    }

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Template");

        worksheet.Cell(1, 1).Value = "seq";
        worksheet.Cell(1, 2).Value = "phone_number";
        worksheet.Cell(1, 3).Value = "remark";
        worksheet.Cell(1, 4).Value = "whatsapp_status";
        worksheet.Cell(1, 5).Value = "agent_name";
        worksheet.Cell(1, 6).Value = "reference";

        // Example row
        worksheet.Cell(2, 1).Value = "1";
        worksheet.Cell(2, 2).Value = "081234567890";
        worksheet.Cell(2, 3).Value = "Example remark";
        worksheet.Cell(2, 4).Value = "active";
        worksheet.Cell(2, 5).Value = "John Doe";
        worksheet.Cell(2, 6).Value = "REF-001";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        var exampleRow = worksheet.Row(2);
        exampleRow.Style.Fill.BackgroundColor = XLColor.LightYellow;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "phone_import_template.xlsx");
    }

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
    public async Task<IActionResult> ExportBatch(ulong id, CancellationToken cancellationToken)
    {
        var roleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        if (!await _permissionService.HasExportPermissionAsync(roleName, "Imports", cancellationToken))
            return Forbid();

        // Get batch info only (without loading rows)
        var batch = await _importQueryService.GetBatchDetailAsync(id, 1, 1, null, cancellationToken);
        if (batch is null)
            return NotFound();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ImportRows");

        // Headers
        ws.Cell(1, 1).Value = "seq";
        ws.Cell(1, 2).Value = "raw_phone_number";
        ws.Cell(1, 3).Value = "normalized_phone_number";
        ws.Cell(1, 4).Value = "remark";
        ws.Cell(1, 5).Value = "row_status";
        ws.Cell(1, 6).Value = "message";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Stream rows in batches of 5000 to avoid loading everything into memory
        int exportPage = 1;
        const int exportPageSize = 5000;
        int row = 2;
        while (true)
        {
            var pageBatch = await _importQueryService.GetBatchDetailAsync(id, exportPage, exportPageSize, null, cancellationToken);
            if (pageBatch is null || !pageBatch.Rows.Any())
                break;

            foreach (var r in pageBatch.Rows)
            {
                ws.Cell(row, 1).Value = r.Seq ?? string.Empty;
                ws.Cell(row, 2).Value = r.RawPhoneNumber ?? string.Empty;
                ws.Cell(row, 3).Value = r.NormalizedPhoneNumber ?? string.Empty;
                ws.Cell(row, 4).Value = r.Remark ?? string.Empty;
                ws.Cell(row, 5).Value = r.RowStatus;
                ws.Cell(row, 6).Value = r.Message ?? string.Empty;
                row++;
            }

            if (pageBatch.Rows.Count < exportPageSize)
                break;

            exportPage++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"import_batch_{id}_{batch.Status}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Status(ulong id, CancellationToken cancellationToken)
    {
        var status = await _importQueryService.GetBatchStatusAsync(id, cancellationToken);
        if (status is null)
            return NotFound();

        return Json(status);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ulong id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            await _importService.ConfirmImportAsync(id, userId, cancellationToken);
            TempData["SuccessMessage"] = "Import confirmed. New records have been inserted.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Confirm failed: {ex.Message}";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(ulong id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await _importService.CancelImportAsync(id, userId, cancellationToken);

        TempData["SuccessMessage"] = "Import has been cancelled.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── Update Import ────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateUpdate()
    {
        return View(new UploadImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUpdate(UploadImportViewModel model, CancellationToken cancellationToken)
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
        var batchId = await _importService.CreatePendingBatchAsync(storedFilePath, model.File.FileName, userId, "update", cancellationToken);
        _backgroundQueue.Enqueue(batchId);

        return RedirectToAction(nameof(Detail), new { id = batchId });
    }

    // ── Delete Import ────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult DownloadDeleteTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Template");

        worksheet.Cell(1, 1).Value = "phone_number";
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        worksheet.Column(1).Width = 20;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "delete_import_template.xlsx");
    }

    [HttpGet]
    public IActionResult CreateDelete()
    {
        return View(new UploadImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDelete(UploadImportViewModel model, CancellationToken cancellationToken)
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
        var batchId = await _importService.CreatePendingBatchAsync(storedFilePath, model.File.FileName, userId, "delete", cancellationToken);
        _backgroundQueue.Enqueue(batchId);

        return RedirectToAction(nameof(Detail), new { id = batchId });
    }

    // ── Web Status Import ────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult DownloadWebStatusTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Template");

        var headers = new[] { "phone_number", "web1", "web2", "web3", "web4", "web5", "web6", "web7", "web8", "web9", "web10",
                              "web11", "web12", "web13", "web14", "web15", "web16", "web17", "web18", "web19", "web20" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "web_status_import_template.xlsx");
    }

    [HttpGet]
    public IActionResult CreateWebStatus()
    {
        return View(new UploadImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWebStatus(UploadImportViewModel model, CancellationToken cancellationToken)
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
        var batchId = await _importService.CreatePendingBatchAsync(storedFilePath, model.File.FileName, userId, "web_status", cancellationToken);
        _backgroundQueue.Enqueue(batchId);

        return RedirectToAction(nameof(Detail), new { id = batchId });
    }
}
