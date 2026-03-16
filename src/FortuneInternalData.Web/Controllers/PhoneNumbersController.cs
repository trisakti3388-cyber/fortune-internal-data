using System.Security.Claims;
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

    public PhoneNumbersController(IPhoneNumberService phoneNumberService)
    {
        _phoneNumberService = phoneNumberService;
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
            filter.Page,
            filter.PageSize,
            cancellationToken);

        return View(filter);
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
            Remark = item.Remark
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
            Remark = model.Remark
        }, userId, cancellationToken);

        TempData["SuccessMessage"] = "Phone number record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PolicyNames.AdminOrAbove)]
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
    [Authorize(Policy = PolicyNames.AdminOrAbove)]
    public async Task<IActionResult> BatchUpdate(BatchUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (model.SelectedIds == null || !model.SelectedIds.Any())
        {
            TempData["ErrorMessage"] = "No records selected for update.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(model.Status) && string.IsNullOrEmpty(model.WhatsappStatus))
        {
            TempData["ErrorMessage"] = "Please select at least one field to update.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await _phoneNumberService.BatchUpdateAsync(model.SelectedIds, model.Status, model.WhatsappStatus, userId, cancellationToken);

        TempData["SuccessMessage"] = $"{model.SelectedIds.Count} record(s) updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
