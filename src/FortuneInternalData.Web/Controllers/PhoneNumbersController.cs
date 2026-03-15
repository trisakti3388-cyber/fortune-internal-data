using FortuneInternalData.Application.DTOs;
using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Web.Security;
using FortuneInternalData.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

[Authorize(Policy = PolicyNames.StaffOrAbove)]
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
        filter.Result = await _phoneNumberService.SearchAsync(
            filter.SearchPhoneNumber,
            filter.Status,
            filter.WhatsappStatus,
            filter.Page,
            filter.PageSize,
            cancellationToken);

        return View(filter);
    }

    [HttpPost]
    public async Task<IActionResult> Update(PhoneNumberUpdateDto request, CancellationToken cancellationToken)
    {
        // TODO: Replace demo user id with authenticated user claim parsing.
        await _phoneNumberService.UpdateAsync(request, userId: 1, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
