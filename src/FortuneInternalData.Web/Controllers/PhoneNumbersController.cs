using System.Security.Claims;
using ClosedXML.Excel;
using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Domain.Constants;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize]
public class PhoneNumbersController : Controller
{
    private readonly IPhoneNumberService _phoneNumberService;
    private readonly IPermissionService _permissionService;

    public PhoneNumbersController(IPhoneNumberService phoneNumberService, IPermissionService permissionService)
    {
        _phoneNumberService = phoneNumberService;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PhoneNumberListViewModel filter, CancellationToken cancellationToken)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        filter.Result = await _phoneNumberService.SearchAsync(
            filter.SearchPhoneNumber,
            filter.Status,
            filter.WhatsappStatus,
            filter.SearchRemark,
            filter.DateFrom,
            filter.DateTo,
            filter.Page,
            filter.PageSize,
            cancellationToken);

        return View(filter);
    }

    [HttpGet]
    public async Task<IActionResult> ExportSearch([FromQuery] PhoneNumberListViewModel filter, CancellationToken cancellationToken)
    {
        var roleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        if (!await _permissionService.HasExportPermissionAsync(roleName, "PhoneData", cancellationToken))
            return Forbid();

        var items = await _phoneNumberService.SearchAllAsync(
            filter.SearchPhoneNumber,
            filter.Status,
            filter.WhatsappStatus,
            filter.SearchRemark,
            filter.DateFrom,
            filter.DateTo,
            cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("PhoneData");

        // Headers
        ws.Cell(1, 1).Value = "seq";
        ws.Cell(1, 2).Value = "phone_number";
        ws.Cell(1, 3).Value = "status";
        ws.Cell(1, 4).Value = "whatsapp_status";
        ws.Cell(1, 5).Value = "remark";
        ws.Cell(1, 6).Value = "agent_name";
        ws.Cell(1, 7).Value = "reference";
        ws.Cell(1, 8).Value = "upload_date";
        ws.Cell(1, 9).Value = "modified_date";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 2;
        foreach (var item in items)
        {
            ws.Cell(row, 1).Value = item.Seq ?? string.Empty;
            ws.Cell(row, 2).Value = item.PhoneNumber;
            ws.Cell(row, 3).Value = item.Status;
            ws.Cell(row, 4).Value = item.WhatsappStatus ?? string.Empty;
            ws.Cell(row, 5).Value = item.Remark ?? string.Empty;
            ws.Cell(row, 6).Value = item.AgentName ?? string.Empty;
            ws.Cell(row, 7).Value = item.Reference ?? string.Empty;
            ws.Cell(row, 8).Value = item.UploadDate.HasValue ? item.UploadDate.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty;
            ws.Cell(row, 9).Value = item.ModifiedDate.HasValue ? item.ModifiedDate.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"phone_data_export_{DateTime.UtcNow.AddHours(7):yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id, CancellationToken cancellationToken)
    {
        var item = await _phoneNumberService.GetByIdAsync(id, cancellationToken);
        if (item == null) return NotFound();

        return View(new PhoneNumberEditViewModel
        {
            Id = item.Id,
            PhoneNumber = item.PhoneNumber,
            Status = item.Status,
            WhatsappStatus = item.WhatsappStatus,
            Remark = item.Remark,
            AgentName = item.AgentName,
            Reference = item.Reference
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PhoneNumberEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        await _phoneNumberService.UpdateAsync(new PhoneNumberUpdateDto
        {
            Id = model.Id,
            Status = model.Status,
            WhatsappStatus = model.WhatsappStatus,
            Remark = model.Remark,
            AgentName = model.AgentName,
            Reference = model.Reference
        }, userId, cancellationToken);

        TempData["SuccessMessage"] = "Phone number record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchDelete(List<ulong> selectedIds, CancellationToken cancellationToken)
    {
        if (selectedIds == null || !selectedIds.Any())
        {
            TempData["ErrorMessage"] = "No records selected for deletion.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await _phoneNumberService.BatchDeleteAsync(selectedIds, userId, cancellationToken);

        TempData["SuccessMessage"] = $"{selectedIds.Count} record(s) deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchUpdate(BatchUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (model.SelectedIds == null || !model.SelectedIds.Any())
        {
            TempData["ErrorMessage"] = "No records selected for update.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(model.Status) && string.IsNullOrEmpty(model.WhatsappStatus)
            && string.IsNullOrEmpty(model.AgentName) && string.IsNullOrEmpty(model.Remark)
            && string.IsNullOrEmpty(model.Reference))
        {
            TempData["ErrorMessage"] = "Please select at least one field to update.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await _phoneNumberService.BatchUpdateAsync(model.SelectedIds, model.Status, model.WhatsappStatus, model.AgentName, model.Remark, model.Reference, userId, cancellationToken);

        TempData["SuccessMessage"] = $"{model.SelectedIds.Count} record(s) updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
