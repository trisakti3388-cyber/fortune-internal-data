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

        var isSuperadmin = User.IsInRole(Roles.Superadmin);

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

        int col = 1;
        ws.Cell(1, col++).Value = "seq";
        ws.Cell(1, col++).Value = "phone_number";
        ws.Cell(1, col++).Value = "status";
        ws.Cell(1, col++).Value = "whatsapp_status";
        ws.Cell(1, col++).Value = "remark";
        ws.Cell(1, col++).Value = "agent_name";
        ws.Cell(1, col++).Value = "reference";
        ws.Cell(1, col++).Value = "upload_date";
        ws.Cell(1, col++).Value = "modified_date";
        if (isSuperadmin)
        {
            for (int w = 1; w <= 10; w++)
                ws.Cell(1, col++).Value = $"web{w}";
        }

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 2;
        foreach (var item in items)
        {
            int c = 1;
            ws.Cell(row, c++).Value = item.Seq ?? string.Empty;
            ws.Cell(row, c++).Value = item.PhoneNumber;
            ws.Cell(row, c++).Value = item.Status;
            ws.Cell(row, c++).Value = item.WhatsappStatus ?? string.Empty;
            ws.Cell(row, c++).Value = item.Remark ?? string.Empty;
            ws.Cell(row, c++).Value = item.AgentName ?? string.Empty;
            ws.Cell(row, c++).Value = item.Reference ?? string.Empty;
            ws.Cell(row, c++).Value = item.UploadDate.HasValue ? item.UploadDate.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty;
            ws.Cell(row, c++).Value = item.ModifiedDate.HasValue ? item.ModifiedDate.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty;
            if (isSuperadmin)
            {
                ws.Cell(row, c++).Value = item.Web1 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web2 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web3 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web4 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web5 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web6 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web7 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web8 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web9 ?? string.Empty;
                ws.Cell(row, c++).Value = item.Web10 ?? string.Empty;
            }
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
            Reference = item.Reference,
            Web1 = item.Web1, Web2 = item.Web2, Web3 = item.Web3, Web4 = item.Web4, Web5 = item.Web5,
            Web6 = item.Web6, Web7 = item.Web7, Web8 = item.Web8, Web9 = item.Web9, Web10 = item.Web10
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PhoneNumberEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var isSuperadmin = User.IsInRole(Roles.Superadmin);

        await _phoneNumberService.UpdateAsync(new PhoneNumberUpdateDto
        {
            Id = model.Id,
            Status = model.Status,
            WhatsappStatus = model.WhatsappStatus,
            Remark = model.Remark,
            AgentName = model.AgentName,
            Reference = model.Reference,
            Web1 = isSuperadmin ? model.Web1 : null,
            Web2 = isSuperadmin ? model.Web2 : null,
            Web3 = isSuperadmin ? model.Web3 : null,
            Web4 = isSuperadmin ? model.Web4 : null,
            Web5 = isSuperadmin ? model.Web5 : null,
            Web6 = isSuperadmin ? model.Web6 : null,
            Web7 = isSuperadmin ? model.Web7 : null,
            Web8 = isSuperadmin ? model.Web8 : null,
            Web9 = isSuperadmin ? model.Web9 : null,
            Web10 = isSuperadmin ? model.Web10 : null,
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

        var isSuperadmin = User.IsInRole(Roles.Superadmin);

        if (string.IsNullOrEmpty(model.Status) && string.IsNullOrEmpty(model.WhatsappStatus)
            && string.IsNullOrEmpty(model.AgentName) && string.IsNullOrEmpty(model.Remark)
            && string.IsNullOrEmpty(model.Reference)
            && (!isSuperadmin || (string.IsNullOrEmpty(model.Web1) && string.IsNullOrEmpty(model.Web2) && string.IsNullOrEmpty(model.Web3)
                && string.IsNullOrEmpty(model.Web4) && string.IsNullOrEmpty(model.Web5) && string.IsNullOrEmpty(model.Web6)
                && string.IsNullOrEmpty(model.Web7) && string.IsNullOrEmpty(model.Web8) && string.IsNullOrEmpty(model.Web9)
                && string.IsNullOrEmpty(model.Web10))))
        {
            TempData["ErrorMessage"] = "Please select at least one field to update.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        await _phoneNumberService.BatchUpdateAsync(
            model.SelectedIds, model.Status, model.WhatsappStatus, model.AgentName, model.Remark, model.Reference,
            isSuperadmin ? model.Web1 : null, isSuperadmin ? model.Web2 : null, isSuperadmin ? model.Web3 : null,
            isSuperadmin ? model.Web4 : null, isSuperadmin ? model.Web5 : null, isSuperadmin ? model.Web6 : null,
            isSuperadmin ? model.Web7 : null, isSuperadmin ? model.Web8 : null, isSuperadmin ? model.Web9 : null,
            isSuperadmin ? model.Web10 : null,
            userId, cancellationToken);

        TempData["SuccessMessage"] = $"{model.SelectedIds.Count} record(s) updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
